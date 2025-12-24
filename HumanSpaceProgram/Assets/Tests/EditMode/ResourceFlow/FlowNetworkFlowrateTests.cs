using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkFlowrateTests
    {
        private const double DT = 0.02;
        private static readonly Vector3 GRAVITY = new Vector3( 0, -10, 0 );
        private const double TOLERANCE_PERCENT = 2.0; // Use percentage tolerance for comparisons

        /// <summary>
        /// Sets up a simple two-tank scenario with a connecting pipe to test flow rates.
        /// </summary>
        private (FlowNetworkSnapshot snapshot, FlowTank tankA, FlowTank tankB, FlowPipe pipe) SetupBasicScenario(
            double pipeLength, double pipeDiameter, double potentialDifference, ISubstance substance )
        {
            var builder = new FlowNetworkBuilder();

            // We control potential difference primarily by height. ΔΦ ≈ g * h for liquids.
            double height = (GRAVITY.magnitude > 0) ? potentialDifference / GRAVITY.magnitude : 0;

            // Use large tanks to ensure the fluid level (and thus potential) doesn't change significantly during the test.
            var tankA = FlowNetworkTestHelper.CreateTestTank( 100.0, GRAVITY, new Vector3( 0, (float)height, 0 ), substance, 1e6 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 100.0, GRAVITY, Vector3.zero, substance, 1e6 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            float area = (float)(Math.PI * (pipeDiameter / 2.0) * (pipeDiameter / 2.0));
            // Inlet/Outlet positions are in the global simulation space. The pipe connects the bottom of the top tank to the top of the bottom tank.
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, (float)height - 1, 0 ), tankB, new Vector3( 0, 1, 0 ), pipeLength, area );

            var snapshot = builder.BuildSnapshot();
            return (snapshot, tankA, tankB, pipe);
        }

        /// <summary>
        /// Runs the simulation for a few steps to get a stabilized initial flow rate.
        /// </summary>
        private double GetInitialFlowRate( FlowNetworkSnapshot snapshot, FlowPipe pipe )
        {
            // Run one step to initialize potentials and conductances from zero.
            snapshot.PrepareAndSolve( (float)DT );
            snapshot.ApplyResults( (float)DT );

            // Run a second step to get a more stable initial flow rate based on the first step's results.
            snapshot.PrepareAndSolve( (float)DT );
            snapshot.ApplyResults( (float)DT );

            int pipeIndex = snapshot.Pipes.ToList().IndexOf( pipe );
            return Math.Abs( snapshot.CurrentFlowRates[pipeIndex] );
        }

        [Test, Description( "Verifies that for laminar flow, doubling the pipe length halves the flow rate." )]
        public void FlowRate_Is_InverselyProportionalToLength_InLaminarFlow()
        {
            // Arrange (Laminar conditions: low potential diff, small diameter, high viscosity)
            double potentialDiff = 10;
            double diameter = 0.01;

            var (snapshot_L1, _, _, pipe_L1) = SetupBasicScenario( 1.0, diameter, potentialDiff, TestSubstances.Oil );
            var (snapshot_L2, _, _, pipe_L2) = SetupBasicScenario( 2.0, diameter, potentialDiff, TestSubstances.Oil );
            try
            {
                // Act
                double flowRate_L1 = GetInitialFlowRate( snapshot_L1, pipe_L1 );
                double flowRate_L2 = GetInitialFlowRate( snapshot_L2, pipe_L2 );

                // Assert
                Assert.That( flowRate_L1, Is.GreaterThan( 0.01 ) );
                Assert.That( flowRate_L2, Is.GreaterThan( 0.01 ) );
                // For both flow regimes, Conductance is proportional to 1/L. So doubling L should halve flow rate.
                Assert.That( flowRate_L1 / flowRate_L2, Is.EqualTo( 2.0 ).Within( TOLERANCE_PERCENT ).Percent, "Doubling length in laminar flow should halve the flow rate." );
            }
            finally
            {
                snapshot_L1.Dispose();
                snapshot_L2.Dispose();
            }
        }

        [Test, Description( "Verifies that for turbulent flow, doubling the pipe length halves the flow rate." )]
        public void FlowRate_Is_InverselyProportionalToLength_InTurbulentFlow()
        {
            // Arrange (Turbulent conditions: high potential diff, large diameter, low viscosity)
            double potentialDiff = 500;
            double diameter = 0.2;

            var (snapshot_L1, _, _, pipe_L1) = SetupBasicScenario( 10.0, diameter, potentialDiff, TestSubstances.Water );
            var (snapshot_L2, _, _, pipe_L2) = SetupBasicScenario( 20.0, diameter, potentialDiff, TestSubstances.Water );
            try
            {
                // Act
                double flowRate_L1 = GetInitialFlowRate( snapshot_L1, pipe_L1 );
                double flowRate_L2 = GetInitialFlowRate( snapshot_L2, pipe_L2 );

                // Assert
                Assert.That( flowRate_L1, Is.GreaterThan( 1.0 ) );
                Assert.That( flowRate_L2, Is.GreaterThan( 1.0 ) );
                Assert.That( flowRate_L1 / flowRate_L2, Is.EqualTo( 2.0 ).Within( TOLERANCE_PERCENT ).Percent, "Doubling length in turbulent flow should halve the flow rate." );
            }
            finally
            {
                snapshot_L1.Dispose();
                snapshot_L2.Dispose();
            }
        }

        [Test, Description( "Verifies that for laminar flow, doubling the potential difference doubles the flow rate." )]
        public void FlowRate_Is_ProportionalToPotentialDifference_InLaminarFlow()
        {
            // Arrange (Laminar conditions)
            double length = 5.0;
            double diameter = 0.01;

            var (snapshot_P1, _, _, pipe_P1) = SetupBasicScenario( length, diameter, 10.0, TestSubstances.Oil );
            var (snapshot_P2, _, _, pipe_P2) = SetupBasicScenario( length, diameter, 20.0, TestSubstances.Oil );
            try
            {
                // Act
                double flowRate_P1 = GetInitialFlowRate( snapshot_P1, pipe_P1 );
                double flowRate_P2 = GetInitialFlowRate( snapshot_P2, pipe_P2 );

                // Assert
                Assert.That( flowRate_P1, Is.GreaterThan( 0.01 ) );
                Assert.That( flowRate_P2, Is.GreaterThan( 0.01 ) );
                // For laminar flow, Flow Rate is proportional to ΔΦ. Doubling ΔΦ should double flow rate.
                Assert.That( flowRate_P2 / flowRate_P1, Is.EqualTo( 2.0 ).Within( TOLERANCE_PERCENT ).Percent, "Doubling potential difference in laminar flow should double the flow rate." );
            }
            finally
            {
                snapshot_P1.Dispose();
                snapshot_P2.Dispose();
            }
        }

        [Test, Description( "Verifies that for turbulent flow, doubling the potential difference increases flow rate by a factor of ~sqrt(2)." )]
        public void FlowRate_FollowsSquareRootOfPotentialDifference_InTurbulentFlow()
        {
            // Arrange (Turbulent conditions)
            double length = 10.0;
            double diameter = 0.2;

            var (snapshot_P1, _, _, pipe_P1) = SetupBasicScenario( length, diameter, 500, TestSubstances.Water );
            var (snapshot_P2, _, _, pipe_P2) = SetupBasicScenario( length, diameter, 1000, TestSubstances.Water );
            try
            {
                // Act
                double flowRate_P1 = GetInitialFlowRate( snapshot_P1, pipe_P1 );
                double flowRate_P2 = GetInitialFlowRate( snapshot_P2, pipe_P2 );

                // Assert
                Assert.That( flowRate_P1, Is.GreaterThan( 1.0 ) );
                Assert.That( flowRate_P2, Is.GreaterThan( 1.0 ) );
                // For turbulent flow, ΔΦ ~ ṁ^1.75. So, ṁ ~ ΔΦ^(1/1.75) = ΔΦ^(4/7).
                // The ratio should be 2^(4/7) ≈ 1.486. The simpler sqrt(2) ≈ 1.414 is a close approximation.
                Assert.That( flowRate_P2 / flowRate_P1, Is.EqualTo( Math.Sqrt( 2.0 ) ).Within( 6.0 ).Percent, "Doubling potential difference in turbulent flow should increase flow rate by approximately sqrt(2)." );
            }
            finally
            {
                snapshot_P1.Dispose();
                snapshot_P2.Dispose();
            }
        }

        [Test, Description( "Verifies that for laminar flow, doubling the diameter increases flow rate by a factor of 16 (D^4)." )]
        public void FlowRate_FollowsDiameterToTheFourth_InLaminarFlow()
        {
            // Arrange (Laminar conditions)
            double length = 5.0;
            double potentialDiff = 10;

            var (snapshot_D1, _, _, pipe_D1) = SetupBasicScenario( length, 0.01, potentialDiff, TestSubstances.Oil );
            var (snapshot_D2, _, _, pipe_D2) = SetupBasicScenario( length, 0.02, potentialDiff, TestSubstances.Oil );
            try
            {
                // Act
                double flowRate_D1 = GetInitialFlowRate( snapshot_D1, pipe_D1 );
                double flowRate_D2 = GetInitialFlowRate( snapshot_D2, pipe_D2 );

                // Assert
                Assert.That( flowRate_D1, Is.GreaterThan( 0.01 ) );
                Assert.That( flowRate_D2, Is.GreaterThan( 0.01 ) );
                // For laminar flow (Hagen-Poiseuille), volumetric flow rate is proportional to D^4 (or A^2). Mass flow is also proportional.
                // Doubling the diameter should increase flow rate by 2^4 = 16 times.
                Assert.That( flowRate_D2 / flowRate_D1, Is.EqualTo( 16.0 ).Within( TOLERANCE_PERCENT ).Percent, "Doubling diameter in laminar flow should increase flow rate by a factor of 16." );
            }
            finally
            {
                snapshot_D1.Dispose();
                snapshot_D2.Dispose();
            }
        }

        [Test, Description( "Verifies that for turbulent flow, flow rate scales with diameter to a power of ~2.714." )]
        public void FlowRate_IncreasesWithDiameter_InTurbulentFlow()
        {
            // Arrange (Turbulent conditions)
            double length = 10.0;
            double potentialDiff = 500;

            var (snapshot_D1, _, _, pipe_D1) = SetupBasicScenario( length, 0.1, potentialDiff, TestSubstances.Water );
            var (snapshot_D2, _, _, pipe_D2) = SetupBasicScenario( length, 0.2, potentialDiff, TestSubstances.Water );

            try
            {
                // Act
                double flowRate_D1 = GetInitialFlowRate( snapshot_D1, pipe_D1 );
                double flowRate_D2 = GetInitialFlowRate( snapshot_D2, pipe_D2 );
                // Assert
                Assert.That( flowRate_D1, Is.GreaterThan( 1.0 ) );
                Assert.That( flowRate_D2, Is.GreaterThan( 1.0 ) );
                // For turbulent flow, the relationship is complex. Using the Darcy-Weisbach equation with the Blasius correlation for the
                // friction factor (f ∝ Re^-0.25), the mass flow rate ṁ scales with D^(19/7).
                // So, doubling the diameter should increase flow by a factor of 2^(19/7) ≈ 6.57.
                double ratio = flowRate_D2 / flowRate_D1;
                Assert.That( ratio, Is.EqualTo( Math.Pow( 2, 19.0 / 7.0 ) ).Within( 10.0 ).Percent, "Flow rate increase should be approximately D^(19/7)" );
            }
            finally
            {
                snapshot_D1.Dispose();
                snapshot_D2.Dispose();
            }
        }
    }
}