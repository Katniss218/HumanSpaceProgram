using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using UnityEngine;
using HSP_Tests;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    [TestFixture]
    public class DirectFlowNetworkIntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TwoTanks_EqualizeLevels()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( TestSubstances.Water, 1000 ); // Full

            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            var ownerA = new object();
            var ownerB = new object();
            var ownerPipe = new object();

            builder.TryAddFlowObj( ownerA, tankA );
            builder.TryAddFlowObj( ownerB, tankB );

            var portA = new FlowPipe.Port( (IResourceConsumer)tankA, new Vector3( 0, -1, 0 ), 0.1f );
            var portB = new FlowPipe.Port( (IResourceConsumer)tankB, new Vector3( 0, -1, 0 ), 0.1f );
            var pipe = new FlowPipe( portA, portB, 1.0, 0.1f ); // Use length and area

            builder.TryAddFlowObj( ownerPipe, pipe );

            var snapshot = builder.BuildSnapshot();

            // Act
            float simulationTime = 10f;
            float fixedDeltaTime = 0.02f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( fixedDeltaTime );
                tankA.ApplyFlows( fixedDeltaTime ); // Apply flows to update tank state
                tankB.ApplyFlows( fixedDeltaTime );
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            Assert.That( massA, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank A should have half the mass." );
            Assert.That( massB, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank B should have half the mass." );
            Assert.That( massA + massB, Is.EqualTo( 1000.0 ).Within( 1e-9 ), "Mass should be conserved." );
        }

        [Test]
        public void Engine_DrainsTank()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tank.Contents.Add( TestSubstances.Kerosene, 800 ); // Full

            var engineFeed = new EngineFeedSystem();
            // Simulate an engine at full throttle demanding pressure
            engineFeed.TargetPressure = 10e5; // 10 bar, a typical RequiredInletPressure
            engineFeed.ExpectedDensity = TestSubstances.Kerosene.GetDensityAtSTP();

            var ownerTank = new object();
            var ownerEngine = new object();
            var ownerPipe = new object();

            builder.TryAddFlowObj( ownerTank, tank );
            builder.TryAddFlowObj( ownerEngine, engineFeed );

            var portTank = new FlowPipe.Port( (IResourceConsumer)tank, new Vector3( 0, -1, 0 ), 0.1f );
            var portEngine = new FlowPipe.Port( engineFeed, new Vector3( 0, 1, 0 ), 0.1f );
            var pipe = new FlowPipe( portTank, portEngine, 1.0, 0.01 ); // Use length and area

            builder.TryAddFlowObj( ownerPipe, pipe );

            var snapshot = builder.BuildSnapshot();
            double massBefore = tank.Contents.GetMass();

            // Act: First step to initiate flow
            snapshot.Step( 0.02f );
            tank.ApplyFlows( 0.02f );
            engineFeed.ApplyFlows( 0.02f ); // Engine processes its inflow

            // Assert 1: Flow started
            double massAfterStep1 = tank.Contents.GetMass();
            Assert.That( massAfterStep1, Is.LessThan( massBefore ), "Tank should have lost mass." );
            Assert.That( engineFeed.ActualMassFlow_LastStep, Is.GreaterThan( 0.0 ), "Engine ActualMassFlow_LastStep should be positive." );

            // Act 2: Second step to ensure consumption continues
            snapshot.Step( 0.02f );
            tank.ApplyFlows( 0.02f );
            engineFeed.ApplyFlows( 0.02f );

            // Assert 2: Consumption occurred
            Assert.That( engineFeed.ActualMassFlow_LastStep, Is.GreaterThan( 0.0 ), "Engine should continue to have positive mass flow." );
            double massAfterStep2 = tank.Contents.GetMass();
            Assert.That( massAfterStep2, Is.LessThan( massAfterStep1 ), "Tank mass should continue to decrease." );
        }
    }
}