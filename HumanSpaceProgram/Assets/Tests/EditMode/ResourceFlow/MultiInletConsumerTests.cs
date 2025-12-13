using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using System;
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

            // Act
            snapshot.Step( (float)DT ); // This triggers UpdateConductances internally

            // Assert
            double density = substance.GetDensityAtSTP();
            double viscosity = substance.GetViscosity( 293, 101325 ); // Assume STP for test substances
            double expectedConductance = FlowEquations.GetLaminarMassConductance( density, area, length, viscosity );

            Assert.That( pipe.MassFlowConductance, Is.EqualTo( expectedConductance ).Within( 1.0 ).Percent );
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

            // Act
            snapshot.Step( (float)DT );

            // Assert
            double density = substance.GetDensityAtSTP();
            double viscosity = substance.GetViscosity( 293, 101325 );
            double reynolds = FlowEquations.GetReynoldsNumber( lastMassFlow, diameter, viscosity );
            double frictionFactor = FlowEquations.GetDarcyFrictionFactor( reynolds );
            double expectedConductance = FlowEquations.GetTurbulentMassConductance( density, area, diameter, length, frictionFactor, lastMassFlow );

            // Note: The solver smooths conductance. We test against the first-step value.
            Assert.That( pipe.MassFlowConductance, Is.EqualTo( expectedConductance ).Within( 1.0 ).Percent );
        }

        [Test, Description( "Verifies that the solver correctly caps conductance at the sonic limit for choked gas flow." )]
        public void UpdateConductances_ChokedGasFlow_IsCorrectlyCapped()
        {
            // Arrange
            var substance = TestSubstances.Air;
            double lastMassFlow = 100.0; // High flow to ensure turbulence
            double diameter = 0.1;
            double length = 1.0;
            double area = Math.PI * (diameter / 2.0) * (diameter / 2.0);

            // Setup a very high potential difference to induce choked flow
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 0, 1000, 0 ), substance, 1.2 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero ); // Vacuum effective
            tankB.FluidState = FluidState.Vacuum;

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 999, 0 ), tankB, new Vector3( 0, 1, 0 ), length, (float)area );
            pipe.MassFlowRateLastStep = lastMassFlow;
            pipe.ConductanceLastStep = 0;

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( (float)DT );

            // Assert
            double density = substance.GetDensityAtSTP();
            double speedOfSound = substance.GetSpeedOfSound( 293, 101325 );
            double maxMassFlow = FlowEquations.GetChokedMassFlow( density, area, speedOfSound );

            // The potential difference will be huge. The conductance should be capped such that m_dot = C * dPhi <= maxMassFlow
            double potentialDiff = tankA.Sample( Vector3.zero, area ).FluidSurfacePotential - tankB.Sample( Vector3.zero, area ).FluidSurfacePotential;
            double expectedChokedConductance = maxMassFlow / potentialDiff;

            Assert.That( pipe.MassFlowConductance, Is.EqualTo( expectedChokedConductance ).Within( 1.0 ).Percent, "Conductance should be capped by the sonic flow limit." );
        }
    }
}
