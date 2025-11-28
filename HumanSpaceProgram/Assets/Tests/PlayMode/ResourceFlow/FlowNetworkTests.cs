using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkTests
    {
        private GameObject _manager;
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if( _manager != null )
            {
                Object.Destroy( _manager );
            }
            if( _root != null )
            {
                Object.Destroy( _root );
            }
        }

        private FlowTank CreateTestTank( double volume, Vector3 acceleration, Vector3 offset )
        {
            var tank = new FlowTank( volume );
            var nodes = new[]
            {
                new Vector3( 0, 1, 0 ) + offset,  // Node 0: Top
                new Vector3( 0, -1, 0 ) + offset, // Node 1: Bottom
                new Vector3( 1, 0, 0 ) + offset,  // Node 2: Right
                new Vector3( -1, 0, 0 ) + offset, // Node 3: Left
                new Vector3( 0, 0, 1 ) + offset,  // Node 4: Forward
                new Vector3( 0, 0, -1 ) + offset  // Node 5: Back
            };

            var inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1, 0 ) + offset ), // Attached to Top
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) + offset )  // Attached to Bottom
            };
            tank.SetNodes( nodes, inlets );
            tank.FluidAcceleration = acceleration;
            tank.FluidState = new FluidState( pressure: 101325.0, temperature: 293.15, velocity: 0.0 );
            return tank;
        }

        [UnityTest]
        public IEnumerator TwoTanks_EqualizeLevels_WhenIdenticalAndHorizontal()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;

            _root = new GameObject( "TestRoot" );

            var substance = new MockSubstance( "water", 1000 );

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( substance, 1000 ); // Full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1f, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1f, 0 ) )
            };

            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1f, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1f, 0 ) )
            };

            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[1];
            pipe.ToInlet = wrapperB.Inlets[1];

            yield return new WaitForFixedUpdate(); // Let everything initialize

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            float simulationTime = 10f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            Assert.AreEqual( 500.0, massA, 1.0, "Tank A should have half the mass." );
            Assert.AreEqual( 500.0, massB, 1.0, "Tank B should have half the mass." );
        }

        [UnityTest]
        public IEnumerator TwoTanks_FlowDownhill_WhenOneIsElevated()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            var substance = new MockSubstance( "water", 1000 );
            Vector3 gravity = new Vector3( 0, -10, 0 );

            // Setup Tank A (High Position)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) );
            tankA.Contents.Add( substance, 500 ); // Half full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1f, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1f, 0 ) )
            };

            // Setup Tank B (Low Position)
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, gravity, Vector3.zero );
            // Tank B starts empty
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1f, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1f, 0 ) )
            };

            // Connect Bottom of A to Bottom of B (U-Tube configuration)
            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[1];
            pipe.ToInlet = wrapperB.Inlets[1];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            float simulationTime = 10f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);

            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            // Since A is physically higher, B should fill up until the fluid level matches A's level in world space.
            // Given the height difference, A should empty significantly into B.
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( tankA.Contents.GetMass() ),
                "Fluid should have flowed to the lower tank (B) due to gravity." );

            Assert.That( tankA.Contents.GetMass(), Is.GreaterThan( 0 ),
                "Tank A should not be completely empty (equilibrium state)." );
        }

        [UnityTest]
        public IEnumerator PipeAtTop_DoesNotDrain_WhenFluidLevelIsLow()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );
            var substance = new MockSubstance( "water", 1000 );

            // Setup Tank A (Source) - High up
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 10, 0 ) );
            tankA.Contents.Add( substance, 100 ); // Only 10% full (Level is at bottom of tank)

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(0, 1f, 0)), // Index 0: Top
                new ResourceInlet(1, new Vector3(0, -1f, 0)) // Index 1: Bottom
            };

            // Setup Tank B (Destination) - Low down
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(0, 1f, 0)),
                new ResourceInlet(1, new Vector3(0, -1f, 0))
            };

            // Connect TOP of A to Bottom of B
            // Even though A is high up, the fluid is at the bottom of A, 
            // so it cannot reach the Top pipe to flow out.
            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[0]; // Connecting to Top inlet
            pipe.ToInlet = wrapperB.Inlets[1];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            float simulationTime = 5f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.AreEqual( 100.0, tankA.Contents.GetMass(), 0.1, "Tank A should not lose mass." );
            Assert.AreEqual( 0.0, tankB.Contents.GetMass(), 0.1, "Tank B should not gain mass." );
        }
    }
}
