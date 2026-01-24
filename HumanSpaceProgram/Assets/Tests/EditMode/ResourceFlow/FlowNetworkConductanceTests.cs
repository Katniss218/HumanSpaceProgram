
using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using System;
using System.CodeDom;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkConductanceTests
    {
        private const double DT = 0.02;
        private static readonly Vector3 GRAVITY = new Vector3( 0, -10, 0 );

        private (FlowNetworkSnapshot snapshot, FlowPipe pipe) SetupConductanceTest( ISubstance substance, double length, double diameter, double lastMassFlow )
        {
            var builder = new FlowNetworkBuilder();
            // Use large potential difference to ensure flow, but exact value doesn't matter for conductance test.
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 100, 0 ), substance, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            float area = (float)(Math.PI * (diameter / 2.0) * (diameter / 2.0));
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 99, 0 ), tankB, new Vector3( 0, 1, 0 ), length, area );
            pipe.MassFlowRateLastStep = lastMassFlow;

            var snapshot = builder.BuildSnapshot();
            return (snapshot, pipe);
        }

        [Test, Description( "Verifies that the solver calculates the correct mass conductance for laminar flow." )]
        public void UpdateConductances_LaminarFlow_IsCorrect()
        {
            // Arrange (Laminar conditions: high viscosity, low flow)
            var substance = TestSubstances.Oil;
            double lastMassFlow = 0.01;
            double diameter = 0.01;
            double length = 2.0;
            double area = Math.PI * (diameter / 2.0) * (diameter / 2.0);

            var (snapshot, pipe) = SetupConductanceTest( substance, length, diameter, lastMassFlow );
            pipe.ConductanceLastStep = 0; // Ensure no smoothing for the first step
            try
            {
                // Act
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );

                // Assert
                double density = substance.GetDensityAtSTP();
                double viscosity = substance.GetViscosity( 293, 101325 ); // Assume STP for test substances
                double expectedConductance = FlowEquations.GetLaminarMassConductance( density, area, length, viscosity );

                Assert.That( pipe.MassFlowConductance, Is.EqualTo( expectedConductance ).Within( 1.0 ).Percent );
            }
            finally
            {
                snapshot.Dispose();
            }
        }

        [Test, Description( "Verifies that the solver calculates the correct mass conductance for turbulent flow." )]
        public void UpdateConductances_TurbulentFlow_IsCorrect()
        {
            // Arrange (Turbulent conditions: low viscosity, high flow)
            var substance = TestSubstances.Water;
            double lastMassFlow = 10.0;
            double diameter = 0.1;
            double length = 5.0;
            double area = Math.PI * (diameter / 2.0) * (diameter / 2.0);

            var (snapshot, pipe) = SetupConductanceTest( substance, length, diameter, lastMassFlow );
            pipe.ConductanceLastStep = 0; // Ensure no smoothing for the first step
            try
            {
                // Act
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );

                // Assert
                double density = substance.GetDensityAtSTP();
                double viscosity = substance.GetViscosity( 293, 101325 );
                double reynolds = FlowEquations.GetReynoldsNumber( lastMassFlow, diameter, viscosity );
                double frictionFactor = FlowEquations.GetDarcyFrictionFactor( reynolds );
                double expectedConductance = FlowEquations.GetTurbulentMassConductance( density, area, diameter, length, frictionFactor, lastMassFlow );

                // Note: The solver smooths conductance. We test against the first-step value.
                Assert.That( pipe.MassFlowConductance, Is.EqualTo( expectedConductance ).Within( 1.0 ).Percent );
            }
            finally
            {
                snapshot.Dispose();
            }
        }

        [Test, Description( "Verifies that the solver correctly caps conductance at the sonic limit for choked gas flow." )]
        public void UpdateConductances_ChokedGasFlow_IsCorrectlyCapped()
        {
            // Arrange
            var substance = TestSubstances.Air;
            double diameter = 0.1;
            double length = 1.0;
            double area = Math.PI * (diameter / 2.0) * (diameter / 2.0);
            double initialTemp = 300;

            // Setup a very high potential difference to induce choked flow
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 10.0, Vector3.zero, new Vector3( 0, 1000, 0 ), substance, 10.2 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 100000.0, Vector3.zero, Vector3.zero ); // Vacuum effective
            tankA.FluidState = new FluidState( 0, initialTemp, 0 ); // Set temperature first
            tankA.FluidState = new FluidState( tankA.Contents.GetPressureInVolume( tankA.Volume, tankA.FluidState ), initialTemp, 0 );
            tankB.FluidState = FluidState.Vacuum;

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 999, 0 ), tankB, new Vector3( 0, 1, 0 ), length, (float)area );

            using var snapshot = builder.BuildSnapshot();

            // Act
            // Run a few steps to let values settle
            for( int i = 0; i < 10; i++ )
            {
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );
            }

            // Assert
            // 1. Get properties at actual conditions from Tank A
            double densityA = tankA.Contents.GetAverageDensity( tankA.FluidState.Temperature, tankA.FluidState.Pressure );
            if( densityA < 1e-9 ) densityA = substance.GetDensityAtSTP();

            double speedOfSound = substance.GetSpeedOfSound( tankA.FluidState.Temperature, tankA.FluidState.Pressure );
            double maxMassFlow = FlowEquations.GetChokedMassFlow( densityA, area, speedOfSound );

            // 2. Calculate Potential Difference using the LINEARIZED model (P/rho) used by the solver kernel.
            // The solver normalizes pressures by the density of the fluid flowing through the pipe (densityA).
            var stateA = tankA.Sample( Vector3.zero, area );
            var stateB = tankB.Sample( Vector3.zero, area );

            double potentialA = stateA.GeometricPotential + (stateA.Pressure / densityA);
            double potentialB = stateB.GeometricPotential + (stateB.Pressure / densityA);
            double potentialDiff = Math.Abs( potentialA - potentialB );

            // 3. Expected Conductance cap
            // m_dot = C * deltaPhi <= m_dot_max  =>  C <= m_dot_max / deltaPhi
            double expectedChokedConductance = maxMassFlow / potentialDiff;

            Assert.That( pipe.MassFlowConductance, Is.EqualTo( expectedChokedConductance ).Within( 5.0 ).Percent, "Conductance should be capped by the sonic flow limit." );
        }
    }
}
