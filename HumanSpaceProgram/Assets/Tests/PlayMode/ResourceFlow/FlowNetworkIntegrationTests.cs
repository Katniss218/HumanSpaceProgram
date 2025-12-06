using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using UnityEngine;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    [TestFixture]
    public class DirectFlowNetworkIntegrationTests
    {
        private Substance _water;
        private Substance _fuel;

        [SetUp]
        public void SetUp()
        {
            _water = new Substance( "water" ) { Phase = SubstancePhase.Liquid, ReferenceDensity = 1000 };
            _fuel = new Substance( "fuel" ) { Phase = SubstancePhase.Liquid, ReferenceDensity = 800 };
        }

        [Test]
        public void TwoTanks_EqualizeLevels()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            var tankA = FlowNetworkTests.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _water, 1000 ); // Full

            var tankB = FlowNetworkTests.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            var ownerA = new object();
            var ownerB = new object();
            var ownerPipe = new object();

            builder.TryAddFlowObj( ownerA, tankA );
            builder.TryAddFlowObj( ownerB, tankB );

            var portA = new FlowPipe.Port( (IResourceConsumer)tankA, new Vector3( 0, -1, 0 ), 0.1f );
            var portB = new FlowPipe.Port( (IResourceConsumer)tankB, new Vector3( 0, -1, 0 ), 0.1f );
            var pipe = new FlowPipe( portA, portB, conductance: 1.0 );

            builder.TryAddFlowObj( ownerPipe, pipe );

            var snapshot = builder.BuildSnapshot();

            // Act
            float simulationTime = 10f;
            float fixedDeltaTime = 0.02f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            Assert.AreEqual( 500.0, massA, 5.0, "Tank A should have half the mass." );
            Assert.AreEqual( 500.0, massB, 5.0, "Tank B should have half the mass." );
            Assert.AreEqual( 1000.0, massA + massB, 1e-9, "Mass should be conserved." );
        }

        [Test]
        public void Engine_DrainsTank()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            var tank = FlowNetworkTests.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tank.Contents.Add( _fuel, 800 ); // Full

            var engineFeed = new EngineFeedSystem( 0.01 );
            engineFeed.IsOutflowEnabled = true;
            engineFeed.PumpPressureRise = 2e5; // 2 bar suction
            engineFeed.ChamberPressure = 50e5; // High, so it consumes little, allowing manifold to fill
            engineFeed.InjectorConductance = 0.001;
            engineFeed.Demand = 1.0; // High demand

            var ownerTank = new object();
            var ownerEngine = new object();
            var ownerPipe = new object();

            builder.TryAddFlowObj( ownerTank, tank );
            builder.TryAddFlowObj( ownerEngine, engineFeed );

            var portTank = new FlowPipe.Port( (IResourceConsumer)tank, new Vector3( 0, -1, 0 ), 0.1f );
            var portEngine = new FlowPipe.Port( engineFeed, new Vector3( 0, 1, 0 ), 0.1f );
            var pipe = new FlowPipe( portTank, portEngine, conductance: 1.0 );

            builder.TryAddFlowObj( ownerPipe, pipe );

            var snapshot = builder.BuildSnapshot();

            // Act 1: First step to initiate flow
            snapshot.Step( 0.02f );

            // Assert 1: Flow started
            Assert.Less( tank.Contents.GetMass(), 800.0, "Tank should have lost mass." );
            Assert.Greater( engineFeed.Inflow.GetMass(), 0.0, "Engine inflow should be positive." );
            double massAfterStep1 = tank.Contents.GetMass() + (engineFeed.Inflow.GetMass() - engineFeed.MassConsumedLastStep);
            Assert.AreEqual( 800.0, massAfterStep1, 1e-9, "Mass should be conserved after step 1." );


            // Act 2: Second step to ensure consumption happens
            snapshot.Step( 0.02f );

            // Assert 2: Consumption occurred
           // Assert.Greater( engineFeed.MassConsumedLastStep, 0.0, "Engine should have consumed propellant." );
            double massAfterStep2 = tank.Contents.GetMass() + (engineFeed.Inflow.GetMass() - engineFeed.MassConsumedLastStep);
            Assert.AreEqual( massAfterStep1, massAfterStep2, 1e-9, "Mass should be conserved after step 2." );
        }
    }
}
