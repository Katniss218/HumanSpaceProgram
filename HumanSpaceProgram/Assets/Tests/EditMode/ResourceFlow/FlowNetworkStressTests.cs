using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using HSP_Tests;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkStressTests
    {
        private const double DT = 0.02;

        private static FlowPipe CreateAndAddPipe( FlowNetworkBuilder builder, IResourceConsumer from, Vector3 fromLocation, IResourceConsumer to, Vector3 toLocation, double length, float area = 0.1f )
        {
            var portA = new FlowPipe.Port( from, fromLocation, area );
            var portB = new FlowPipe.Port( to, toLocation, area );
            var pipe = new FlowPipe( portA, portB, length, area );
            builder.TryAddFlowObj( new object(), pipe );
            return pipe;
        }

        [Test]
        public void ConflictingPumps_OnSamePipe_NetFlowIsCorrect()
        {
            // Arrange: Two tanks at equal potential with a pipe between them.
            // Two pumps are on the same pipe, pushing in opposite directions.
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( -2, 0, 0 ) );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 2, 0, 0 ) );
            tankA.Contents.Add( TestSubstances.Water, 500 );
            tankB.Contents.Add( TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( -1, 0, 0 ), tankB, new Vector3( 1, 0, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 50; i++ )
            {
                // Simulate modifiers by directly setting HeadAdded before the step
                pipe.HeadAdded = 200.0 - 50.0; // Net head A -> B
                snapshot.Step( 0.02f );
                tankA.ApplyFlows( DT );
                tankB.ApplyFlows( DT );
            }

            // Assert
            // Net head should be 200 - 50 = 150. Flow should be A -> B.
            Assert.That( tankA.Contents.GetMass(), Is.LessThan( 500.0 ), "Tank A should lose mass due to stronger pump." );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 500.0 ), "Tank B should gain mass." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
        }

        [Test]
        public void ClosedLoopWithPump_MassIsConserved()
        {
            // Arrange: A single tank with a pipe looping back to itself, with a pump.
            // This is a "do-nothing" machine that should just churn fluid.
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 2.0, Vector3.zero, Vector3.zero );
            tank.Contents.Add( TestSubstances.Water, 1000 ); // Half full

            builder.TryAddFlowObj( new object(), tank );

            var pipe = CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), tank, new Vector3( 0, 1, 0 ), 1.0 );
            pipe.HeadAdded = 100.0; // Pump pushes fluid from bottom to top of the same tank.

            var snapshot = builder.BuildSnapshot();
            double initialMass = tank.Contents.GetMass();

            // Act & Assert
            Assert.DoesNotThrow( () =>
            {
                for( int i = 0; i < 100; i++ )
                {
                    snapshot.Step( 0.02f );
                    tank.ApplyFlows( DT );
                }
            }, "Solver threw an exception on a closed loop with a pump." );

            Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-9 ), "Mass must be conserved in a closed loop." );
        }

        [Test]
        public void StiffnessShock_HighPressureIntoNearlyFullSmallTank_ConvergesSafely()
        {
            // Arrange: A large, full, high-pressure tank connected to a tiny, nearly-full tank.
            // This is a stiff system prone to overfilling and oscillation.
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var sourceTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, 10, 0 ), TestSubstances.Water, 10000 );
            var stiffSink = FlowNetworkTestHelper.CreateTestTank( 0.1, gravity, Vector3.zero, TestSubstances.Water, 99.9 );

            builder.TryAddFlowObj( new object(), sourceTank );
            builder.TryAddFlowObj( new object(), stiffSink );

            CreateAndAddPipe( builder, sourceTank, new Vector3( 0, 9, 0 ), stiffSink, new Vector3( 0, 1, 0 ), 0.01, 0.5f ); // High conductance pipe

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( (float)DT );
            sourceTank.ApplyFlows( DT );
            stiffSink.ApplyFlows( DT );

            // Assert
            double massInSink = stiffSink.Contents.GetMass();
            double volumeInSink = massInSink / TestSubstances.Water.ReferenceDensity;
            double totalMass = sourceTank.Contents.GetMass() + massInSink;

            Assert.That( totalMass, Is.EqualTo( 10099.9 ).Within( 1e-6 ), "Mass must be conserved." );
            Assert.That( volumeInSink, Is.LessThanOrEqualTo( stiffSink.Volume + 1e-6 ), "Stiff sink should not catastrophically overfill in a single step." );
            Assert.That( massInSink, Is.GreaterThan( 99.9 ), "Some flow should have occurred." );
        }

        [Test]
        public void EnginePullOnNearEmptyTank_HandlesVacuumSafely()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero );
            double initialMass = 0.1; // Very little fuel
            tank.Contents.Add( TestSubstances.Kerosene, initialMass );

            var engine = new EngineFeedSystem()
            {
                TargetPressure = 50e5, // Strong suction
                ExpectedDensity = TestSubstances.Kerosene.GetDensityAtSTP()
            };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), engine );
            var pipe = CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), engine, Vector3.zero, 1.0, 0.1f );

            var snapshot = builder.BuildSnapshot();
            double totalMassTransferred = 0;

            // Act: Run for a few steps, enough to drain the tank.
            for( int i = 0; i < 20; i++ )
            {
                snapshot.Step( (float)DT );
                // Simulate engine consuming what it receives
                totalMassTransferred += engine.Inflow.GetMass();
                engine.ApplyFlows( DT ); // This clears inflow and sets ActualMassFlow_LastStep
                tank.ApplyFlows( DT );
            }

            // Assert
            Assert.That( tank.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 1e-9 ), "Tank should be completely empty." );
            Assert.That( totalMassTransferred, Is.EqualTo( initialMass ).Within( 1e-9 ), "Total mass transferred to engine should equal initial tank mass." );

            // Check that flow stops once tank is empty
            Assert.That( engine.ActualMassFlow_LastStep, Is.EqualTo( 0.0 ).Within( 1e-9 ), "Flow rate should be zero after the tank is drained." );
        }
    }
}
