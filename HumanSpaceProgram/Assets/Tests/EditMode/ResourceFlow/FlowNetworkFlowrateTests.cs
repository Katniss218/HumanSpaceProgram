
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
        private const double TOLERANCE_PERCENT = 5.0; // 5% tolerance for regime approximations

        /// <summary>
        /// Helper to create a simple source->pipe->sink setup and run it to stability.
        /// </summary>
        private double MeasureSteadyStateFlowRate( ISubstance substance, double pipeLength, double pipeDiameter, double headHeight )
        {
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            // Source Tank: High Pressure (Positioned at headHeight)
            var tankA = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, (float)headHeight, 0 ) );
            tankA.FluidState = FluidState.STP;
            // Fill with enough fluid so it doesn't run out during test
            tankA.Contents.Add( substance, tankA.Volume * substance.GetDensityAtSTP() );

            // Sink Tank: Low Pressure (Positioned at 0)
            var tankB = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, Vector3.zero );
            tankB.FluidState = FluidState.STP;
            tankB.Contents.Add( substance, tankB.Volume * substance.GetDensityAtSTP() * 0.5 ); // Half full

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            // Pipe Area
            float area = (float)(Math.PI * Math.Pow( pipeDiameter / 2.0, 2 ));

            // Connect tanks.
            // Tank A is at Y=headHeight. Port A must be at Y=headHeight to sample the tank correctly.
            // Tank B is at Y=0. Port B is at Y=0.
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, (float)headHeight, 0 ), tankB, Vector3.zero, pipeLength, area );

            using var snapshot = builder.BuildSnapshot();

            // Run simulation to let conductance smoothing stabilize
            // Laminar stabilization is fast, Turbulent can take a moment for the friction factor to settle.
            for( int i = 0; i < 50; i++ )
            {
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );

                // Keep Source full / Sink not full to maintain pressure head (approx)
                // In a perfect test we'd use infinite sources, but resetting mass works.
                tankA.Contents[substance] = tankA.Volume * substance.GetDensityAtSTP();
                tankB.Contents[substance] = tankB.Volume * substance.GetDensityAtSTP() * 0.5;
            }

            return Math.Abs( snapshot.CurrentFlowRates[0] );
        }

        #region Laminar Flow Tests (Hagen-Poiseuille)
        // Law: Flow ~ (DeltaP * D^4) / (L * Viscosity)
        // We use TestSubstances.Oil (Viscosity ~ 1.0) to ensure low Reynolds numbers.

        [Test]
        public void Laminar_DoublePotential_DoublesFlow()
        {
            double d = 0.05; // 5cm
            double l = 10.0;
            double h = 10.0;

            double flow1 = MeasureSteadyStateFlowRate( TestSubstances.Oil, l, d, h );
            double flow2 = MeasureSteadyStateFlowRate( TestSubstances.Oil, l, d, h * 2.0 );

            Assert.That( flow2 / flow1, Is.EqualTo( 2.0 ).Within( TOLERANCE_PERCENT ).Percent );
        }

        [Test]
        public void Laminar_DoubleLength_HalvesFlow()
        {
            double d = 0.05;
            double h = 10.0;
            double l1 = 5.0;
            double l2 = 10.0;

            double flow1 = MeasureSteadyStateFlowRate( TestSubstances.Oil, l1, d, h );
            double flow2 = MeasureSteadyStateFlowRate( TestSubstances.Oil, l2, d, h );

            Assert.That( flow1 / flow2, Is.EqualTo( 2.0 ).Within( TOLERANCE_PERCENT ).Percent );
        }

        [Test]
        public void Laminar_DoubleDiameter_IncreasesFlow16x()
        {
            double d1 = 0.02;
            double d2 = 0.04;
            double l = 5.0;
            double h = 10.0;

            double flow1 = MeasureSteadyStateFlowRate( TestSubstances.Oil, l, d1, h );
            double flow2 = MeasureSteadyStateFlowRate( TestSubstances.Oil, l, d2, h );

            // 2^4 = 16
            Assert.That( flow2 / flow1, Is.EqualTo( 16.0 ).Within( TOLERANCE_PERCENT ).Percent );
        }
        #endregion

        #region Turbulent Flow Tests (Darcy-Weisbach)
        // Law: DeltaP ~ Flow^2  =>  Flow ~ Sqrt(DeltaP)
        // Law: Flow ~ D^2.5 (approx, varies with friction factor correlation)
        // We use TestSubstances.Water (Viscosity ~ 0.001) to ensure high Reynolds numbers.

        [Test]
        public void Turbulent_DoublePotential_IncreasesFlowSqrt2()
        {
            // Water, large pipe, high pressure -> Turbulent
            double d = 0.2;
            double l = 10000.0;
            double h = 100.0; // Large head

            double flow1 = MeasureSteadyStateFlowRate( TestSubstances.Water, l, d, h );
            double flow2 = MeasureSteadyStateFlowRate( TestSubstances.Water, l, d, h * 2.0 );

            double expectedRatio = Math.Sqrt( 2.0 ); // ~1.414
            Assert.That( flow2 / flow1, Is.EqualTo( expectedRatio ).Within( TOLERANCE_PERCENT ).Percent );
        }

        [Test]
        public void Turbulent_DoubleLength_DecreasesFlowSqrt2()
        {
            double d = 0.2;
            double h = 100.0;
            double l1 = 10000.0;
            double l2 = 20000.0;

            double flow1 = MeasureSteadyStateFlowRate( TestSubstances.Water, l1, d, h );
            double flow2 = MeasureSteadyStateFlowRate( TestSubstances.Water, l2, d, h );

            // In turbulent flow, HeadLoss ~ L * V^2.
            // So V^2 ~ 1/L  => V ~ 1/Sqrt(L).
            // Doubling L divides velocity (and mass flow) by Sqrt(2).
            double expectedRatio = Math.Sqrt( 2.0 );
            Assert.That( flow1 / flow2, Is.EqualTo( expectedRatio ).Within( TOLERANCE_PERCENT ).Percent );
        }
        #endregion
    }
}
