using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture, Description(
@"This test fixture investigates a counter-intuitive emergent behavior of the resource flow solver where, under very high pump pressures, a low-conductance pipe can deliver more mass over time than a high-conductance pipe.

The Phenomenon:
The core issue is an interaction between three factors:
1. High Pipe Conductance: Allows for extremely high potential flow rates.
2. High Engine Demand (TargetPressure): Creates a very large potential difference, driving that high flow rate.
3. A 'Stiff' Consumer: The EngineFeedSystem has a small internal volume. A large inflow of incompressible liquid causes its internal pressure to spike dramatically in a single frame.

The sequence of events is as follows:
1. Overshoot: The high-conductance pipe, driven by the engine's demand, injects a massive volume of fluid into the small engine manifold in a single simulation step.
2. Potential Reversal: The solver sees this massive inflow, and on the next iteration, the potential of the (now full) manifold is no longer strongly negative, causing a sharp drop in calculated flow.
3. Oscillation & Damping: The solver's stability algorithm detects this rapid +flow -> low-flow change as a severe oscillation. To stabilize the system, it applies heavy, learned damping specifically to that pipe, drastically reducing its effective flow rate.
4. The 'Slow and Steady' Winner: Meanwhile, the low-conductance pipe trickles propellant in slowly. It never delivers enough mass to cause a pressure spike, so it never oscillates and is never damped.

Result: The consistently flowing (but slow) pipe can deliver more total mass than the heavily damped (but high-conductance) pipe. Lowering the engine's target pressure avoids the initial overshoot, preventing the system from entering this oscillatory state. These tests demonstrate this behavior and the crossover point." )]
    public class PumpAndConductanceTests
    {
        private const double DT = 0.02;

        private (double totalFuel, double totalLox) SimulateFlow( double fuelPipeConductance, double loxPipeConductance, double targetPressure, int simulationSteps )
        {
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -9.81f, 0 );
            double fuelMass = 8000;
            double loxMass = 11410;

            var fuelTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Kerosene, fuelMass );
            var loxTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, 100, 0 ), TestSubstances.Lox, loxMass );
            var fuelFeed = new EngineFeedSystem();
            var loxFeed = new EngineFeedSystem();

            builder.TryAddFlowObj( new object(), fuelTank );
            builder.TryAddFlowObj( new object(), loxTank );
            builder.TryAddFlowObj( new object(), fuelFeed );
            builder.TryAddFlowObj( new object(), loxFeed );

            // Create pipes with specified conductances
            var portA = new FlowPipe.Port( (IResourceProducer)fuelTank, new Vector3( 0, 99, 0 ), 0.1f );
            var portB = new FlowPipe.Port( fuelFeed, new Vector3( 0, 1, 0 ), 0.1f );

            // Reverse-engineer pipe length to achieve desired mass conductance for the test
            double fuelDensity = TestSubstances.Kerosene.ReferenceDensity;
            double fuelViscosity = TestSubstances.Kerosene.GetViscosity( 293, 101325 );
            // C_laminar = (rho^2 * A^2) / (8 * pi * mu * L) => L = ...
            double fuelLength = (fuelDensity * fuelDensity * portA.area * portA.area) / (8 * Math.PI * fuelViscosity * fuelPipeConductance);
            var pipeFuel = new FlowPipe( portA, portB, fuelLength, portA.area );
            builder.TryAddFlowObj( new object(), pipeFuel );

            var portC = new FlowPipe.Port( (IResourceProducer)loxTank, new Vector3( 0, 99, 0 ), 0.1f );
            var portD = new FlowPipe.Port( loxFeed, new Vector3( 0, 1, 0 ), 0.1f );

            double loxDensity = TestSubstances.Lox.ReferenceDensity;
            double loxViscosity = TestSubstances.Lox.GetViscosity( 90, 101325 ); // Approx for LOX
            double loxLength = (loxDensity * loxDensity * portC.area * portC.area) / (8 * Math.PI * loxViscosity * loxPipeConductance);
            var pipeLox = new FlowPipe( portC, portD, loxLength, portC.area );
            builder.TryAddFlowObj( new object(), pipeLox );

            var snapshot = builder.BuildSnapshot();

            double totalFuelTransferred = 0;
            double totalLoxTransferred = 0;

            for( int i = 0; i < simulationSteps; i++ )
            {
                // --- Drive the EngineFeedSystems (simulating FRocketEngine's logic) ---
                fuelFeed.TargetPressure = targetPressure;
                loxFeed.TargetPressure = targetPressure;
                fuelFeed.ExpectedDensity = TestSubstances.Kerosene.GetDensityAtSTP();
                loxFeed.ExpectedDensity = TestSubstances.Lox.GetDensityAtSTP();

                // --- Simulate ---
                snapshot.Step( (float)DT );

                // --- Accumulate transferred mass ---
                // In this test, we care about what the solver delivers, not what an engine "consumes".
                // So we sum up the inflow directly.
                totalFuelTransferred += fuelFeed.Inflow.GetMass();
                totalLoxTransferred += loxFeed.Inflow.GetMass();

                // Manually apply flows to clear buffers for the next step, simulating consumption.
                fuelFeed.ApplyFlows( DT );
                loxFeed.ApplyFlows( DT );
                fuelTank.ApplyFlows( DT ); // Allow tanks to update their internal state too
                loxTank.ApplyFlows( DT );
            }

            return (totalFuelTransferred, totalLoxTransferred);
        }

        [Test, Description( "With a low engine target pressure, the system is not stiff enough to oscillate. The high-conductance pipe correctly delivers much more mass." )]
        public void HighConductanceVsLowConductance_WithLowTargetPressure_HighConductanceWins()
        {
            // Arrange
            double targetPressure = 6e5; // 6 bar
            double fuelConductance = 10.0;
            double loxConductance = 0.0001;

            // Act
            var (totalFuel, totalLox) = SimulateFlow( fuelConductance, loxConductance, targetPressure, 50 );

            // Assert
            Assert.That( totalFuel, Is.GreaterThan( 0 ), "Fuel should have been consumed." );
            Assert.That( totalLox, Is.GreaterThan( 0 ), "LOX should have been consumed." );
            Assert.That( totalFuel, Is.GreaterThan( totalLox * 1000 ), "With low pump pressure, fuel flow should be orders of magnitude greater than restricted LOX flow." );
        }

        [Test, Description( "With a very high engine target pressure, the high-conductance fuel line becomes unstable and is heavily damped by the solver, while the low-conductance LOX line is not. This results in the LOX line delivering more mass over time." )]
        public void HighConductanceVsLowConductance_WithHighTargetPressure_LowConductanceWins()
        {
            // Arrange
            double targetPressure = 60e5; // 60 bar
            double fuelConductance = 10.0;
            double loxConductance = 0.0001;

            // Act
            var (totalFuel, totalLox) = SimulateFlow( fuelConductance, loxConductance, targetPressure, 100 );

            // Assert
            Assert.That( totalFuel, Is.GreaterThan( 0 ), "Fuel should have been consumed." );
            Assert.That( totalLox, Is.GreaterThan( 0 ), "LOX should have been consumed." );
            Assert.That( totalLox, Is.GreaterThan( totalFuel ), "With high pump pressure, solver damping causes the low-conductance LOX line to outperform the high-conductance Fuel line." );
        }

        [TestCase( 6e5, 10.0, 0.0001, ExpectedResult = true, Description = "Low Pressure (6 bar): Fuel flow should dominate." )]
        [TestCase( 20e5, 10.0, 0.0001, ExpectedResult = true, Description = "Medium Pressure (20 bar): Fuel flow should still dominate." )]
        [TestCase( 40e5, 10.0, 0.0001, ExpectedResult = false, Description = "High Pressure (40 bar): Crossover point, LOX flow starts winning." )]
        [TestCase( 60e5, 10.0, 0.0001, ExpectedResult = false, Description = "Very High Pressure (60 bar): LOX flow dominates due to fuel line damping." )]
        public bool FlowRateVsTargetPressure_DemonstratesCrossover( double targetPressure, double fuelConductance, double loxConductance )
        {
            // Act
            var (totalFuel, totalLox) = SimulateFlow( fuelConductance, loxConductance, targetPressure, 100 );

            Debug.Log( $"Target Pressure: {targetPressure / 1e5:F1} bar | Fuel Transferred: {totalFuel:F2} kg | LOX Transferred: {totalLox:F2} kg" );

            // Assert
            return totalFuel > totalLox;
        }

        [Test, Description( "Uses a bisection search to programmatically find the engine target pressure at which the solver's stability damping causes the low-conductance pipe to outperform the high-conductance one." )]
        public void FindsCrossoverPointProgrammatically()
        {
            // Arrange
            const double fuelConductance = 10.0;
            const double loxConductance = 0.0001;
            const int simSteps = 50;

            double lowPressure = 1e5;    // 1 bar
            double highPressure = 100e5; // 100 bar

            Func<double, bool> doesHighConductanceWin = ( pressure ) =>
            {
                var (fuel, lox) = SimulateFlow( fuelConductance, loxConductance, pressure, simSteps );
                return fuel > lox;
            };

            // Act: Perform bisection search for 20 iterations to find the crossover point.
            for( int i = 0; i < 20; i++ )
            {
                double midPressure = lowPressure + (highPressure - lowPressure) / 2.0;
                if( doesHighConductanceWin( midPressure ) )
                {
                    // High-C wins, so the crossover must be at a higher pressure.
                    lowPressure = midPressure;
                }
                else
                {
                    // Low-C wins, so the crossover is at or below this pressure.
                    highPressure = midPressure;
                }
            }

            double crossoverPressure = (lowPressure + highPressure) / 2.0;

            Debug.Log( $"Programmatically found crossover pressure: {crossoverPressure / 1e5:F2} bar" );

            // Assert
            // Based on the TestCase results, we expect the crossover to be between 20 and 40 bar.
            Assert.That( crossoverPressure, Is.GreaterThan( 20e5 ), "Crossover pressure should be above the medium pressure test case." );
            Assert.That( crossoverPressure, Is.LessThan( 40e5 ), "Crossover pressure should be below the high pressure test case." );

            // Verify the behavior on either side of the found crossover point
            Assert.That( doesHighConductanceWin( crossoverPressure - 5e5 ), Is.True, "Slightly below crossover, high-conductance should win." );
            Assert.That( doesHighConductanceWin( crossoverPressure + 5e5 ), Is.False, "Slightly above crossover, low-conductance should win." );
        }
    }
}
