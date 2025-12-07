using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    [Description(
@"This test fixture investigates a counter-intuitive emergent behavior of the resource flow solver where, under very high pump pressures, a low-conductance pipe can deliver more mass over time than a high-conductance pipe.

The Phenomenon:
The core issue is an interaction between three factors:
1. High Pipe Conductance: Allows for extremely high potential flow rates.
2. High Pump Pressure: Creates a very large potential difference, driving that high flow rate.
3. A 'Stiff' Consumer: The EngineFeedSystem has a small internal volume. A large inflow of incompressible liquid causes its internal pressure to spike dramatically in a single frame.

The sequence of events is as follows:
1. Overshoot: The high-conductance pipe, driven by the pump, injects a massive volume of fluid into the small engine manifold in a single simulation step.
2. Pressure Spike & Reversal: The manifold's pressure skyrockets, becoming much higher than the source tank's pressure. In the next step, this reverses the potential gradient, causing the solver to calculate a massive flow *out* of the manifold.
3. Oscillation & Damping: The solver's stability algorithm detects this rapid +flow -> -flow flip-flop as a severe oscillation. To stabilize the system, it applies heavy, learned damping specifically to that pipe, drastically reducing its effective flow rate.
4. The 'Slow and Steady' Winner: Meanwhile, the low-conductance pipe trickles propellant in slowly. It never delivers enough mass to cause a pressure spike, so it never oscillates and is never damped.

Result: The consistently flowing (but slow) pipe can deliver more total mass than the heavily damped (but high-conductance) pipe. Lowering the pump pressure avoids the initial overshoot, preventing the system from entering this oscillatory state. These tests demonstrate this behavior and the crossover point." )]
    public class PumpAndConductanceTests
    {
        private const double DT = 0.02;

        private (double totalFuel, double totalLox) SimulateFlow( double fuelPipeConductance, double loxPipeConductance, double pumpPressure, int simulationSteps )
        {
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -9.81f, 0 );
            double fuelMass = 8000;
            double loxMass = 11410;
            double mixtureRatio = 1;

            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, fuelMass );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Lox, loxMass );
            var fuelFeed = new EngineFeedSystem( 0.01 );
            var loxFeed = new EngineFeedSystem( 0.01 );

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            // Create pipes with specified conductances
            var portA = new FlowPipe.Port( (IResourceConsumer)fuelTank, new Vector3( 0, 99, 0 ), 0.1f );
            var portB = new FlowPipe.Port( fuelFeed, new Vector3( 0, 1, 0 ), 0.1f );
            var pipeFuel = new FlowPipe( portA, portB, fuelPipeConductance );
            builder.TryAddFlowObj( new object(), pipeFuel );

            var portC = new FlowPipe.Port( (IResourceConsumer)loxTank, new Vector3( 0, 99, 0 ), 0.1f );
            var portD = new FlowPipe.Port( loxFeed, new Vector3( 0, 1, 0 ), 0.1f );
            var pipeLox = new FlowPipe( portC, portD, loxPipeConductance );
            builder.TryAddFlowObj( new object(), pipeLox );

            var snapshot = builder.BuildSnapshot();

            double totalFuelConsumed = 0;
            double totalLoxConsumed = 0;

            const double targetTotalMassFlow = 150.0;
            const double chamberPressureToMassFlowRatio = 50e5 / 150.0;
            const double NominalPressureDelta = 1e6;

            for( int i = 0; i < simulationSteps; i++ )
            {
                fuelFeed.IsOutflowEnabled = loxFeed.IsOutflowEnabled = true;

                double fuelMassDemand = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassDemand = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));
                fuelFeed.Demand = fuelMassDemand / TestSubstances.Kerosene.ReferenceDensity;
                loxFeed.Demand = loxMassDemand / TestSubstances.Lox.ReferenceDensity;

                double totalMassConsumedLastStep = fuelFeed.MassConsumedLastStep + loxFeed.MassConsumedLastStep;
                double currentChamberPressure = (totalMassConsumedLastStep > 1e-6) ? (totalMassConsumedLastStep / DT) * chamberPressureToMassFlowRatio : 1e5;
                fuelFeed.ChamberPressure = currentChamberPressure;
                loxFeed.ChamberPressure = currentChamberPressure;

                double fuelMassFlowShare = targetTotalMassFlow * (1.0 / (1.0 + mixtureRatio));
                double loxMassFlowShare = targetTotalMassFlow * (mixtureRatio / (1.0 + mixtureRatio));

                fuelFeed.InjectorConductance = fuelMassFlowShare / Math.Sqrt( NominalPressureDelta );
                loxFeed.InjectorConductance = loxMassFlowShare / Math.Sqrt( NominalPressureDelta );

                fuelFeed.PumpPressureRise = loxFeed.PumpPressureRise = pumpPressure;

                snapshot.Step( (float)DT );

                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );

                totalFuelConsumed += fuelFeed.MassConsumedLastStep;
                totalLoxConsumed += loxFeed.MassConsumedLastStep;
                Debug.Log( totalFuelConsumed / TestSubstances.Kerosene.ReferenceDensity + " : " + totalLoxConsumed / TestSubstances.Lox.ReferenceDensity );
            }

            return (totalFuelConsumed, totalLoxConsumed);
        }

        [Test, Description( "With a low pump head, the system is not stiff enough to oscillate. The high-conductance pipe correctly delivers much more mass." )]
        public void HighConductanceVsLowConductance_WithLowPumpHead_HighConductanceWins()
        {
            // Arrange
            double pumpPressure = 6e4; // 0.6 bar
            double fuelConductance = 10.0;
            double loxConductance = 0.0001;

            // Act
            var (totalFuel, totalLox) = SimulateFlow( fuelConductance, loxConductance, pumpPressure, 50 );

            // Assert
            Assert.Greater( totalFuel, 0, "Fuel should have been consumed." );
            Assert.Greater( totalLox, 0, "LOX should have been consumed." );
            Assert.Greater( totalFuel, totalLox * 1000, "With low pump pressure, fuel flow should be orders of magnitude greater than restricted LOX flow." );
        }

        [Test, Description( "With a very high pump head, the high-conductance fuel line becomes unstable and is heavily damped by the solver, while the low-conductance LOX line is not. This results in the LOX line delivering more mass over time." )]
        public void HighConductanceVsLowConductance_WithHighPumpHead_LowConductanceWins()
        {
            // Arrange
            double pumpPressure = 60e5; // 60 bar
            double fuelConductance = 10.0;
            double loxConductance = 0.0001;

            // Act
            var (totalFuel, totalLox) = SimulateFlow( fuelConductance, loxConductance, pumpPressure, 100 );

            // Assert
            Assert.Greater( totalFuel, 0, "Fuel should have been consumed." );
            Assert.Greater( totalLox, 0, "LOX should have been consumed." );
            Assert.Greater( totalLox, totalFuel, "With high pump pressure, solver damping causes the low-conductance LOX line to outperform the high-conductance Fuel line." );
        }

        [TestCase( 6e4, 10.0, 0.0001, ExpectedResult = true, Description = "Low Pressure (0.6 bar): Fuel flow should dominate." )]
        [TestCase( 20e5, 10.0, 0.0001, ExpectedResult = true, Description = "Medium Pressure (20 bar): Fuel flow should still dominate." )]
        [TestCase( 40e5, 10.0, 0.0001, ExpectedResult = false, Description = "High Pressure (40 bar): Crossover point, LOX flow starts winning." )]
        [TestCase( 60e5, 10.0, 0.0001, ExpectedResult = false, Description = "Very High Pressure (60 bar): LOX flow dominates due to fuel line damping." )]
        public bool FlowRateVsPumpPressure_DemonstratesCrossover( double pumpPressure, double fuelConductance, double loxConductance )
        {
            // Act
            var (totalFuel, totalLox) = SimulateFlow( fuelConductance, loxConductance, pumpPressure, 100 );

            Debug.Log( $"Pump Pressure: {pumpPressure:E2} Pa | Fuel Consumed: {totalFuel:F2} kg | LOX Consumed: {totalLox:F2} kg" );

            // Assert
            return totalFuel > totalLox;
        }
    }
}
