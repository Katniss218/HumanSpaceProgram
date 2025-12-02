using HSP.ResourceFlow;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkTests
    {
        private GameObject _manager;
        private GameObject _root;

        // Standard Test Substances
        private Substance _water;
        private Substance _air;
        private Substance _fuel;
        [SetUp]
        public void SetUp()
        {
            // Initialize standard substances for use in tests
            _water = new Substance( "water" )
            {
                DisplayName = "Water",
                Phase = SubstancePhase.Liquid,
                ReferenceDensity = 1000f,
                ReferencePressure = 101325f,
                BulkModulus = 2.2e9f, // Water is slightly compressible
                DisplayColor = Color.blue
            };

            _air = new Substance( "air" )
            {
                DisplayName = "Air",
                Phase = SubstancePhase.Gas,
                SpecificGasConstant = 287f, // R_specific for Air
                MolarMass = 0.02896,
                DisplayColor = Color.clear
            };

            _fuel = new Substance( "kerosene" )
            {
                DisplayName = "Rocket Fuel",
                Phase = SubstancePhase.Liquid,
                ReferenceDensity = 820f,
                ReferencePressure = 101325f,
                BulkModulus = 1.6e9f,
                DisplayColor = Color.yellow,
                Tags = new[] { "Fuel" }
            };
        }

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

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _water, 1000 ); // Full
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

            Vector3 gravity = new Vector3( 0, -10, 0 );

            // Setup Tank A (High Position)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) );
            tankA.Contents.Add( _water, 500 ); // Half full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1f, 0 ) + new Vector3( 0, 5f, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1f, 0 ) + new Vector3( 0, 5f, 0 ) )
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
            Assert.That( tankA.Contents.GetMass(), Is.GreaterThanOrEqualTo( 0 ) ); // check if flow works correctly and no negatives.
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThanOrEqualTo( 0 ) );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 500 ).Within( 1e-3 ) ); // check if mass doesn't get created out of thin air.
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( tankA.Contents.GetMass() ), "Fluid should have flowed to the lower tank (B) due to gravity." );
        }

        [UnityTest]
        public IEnumerator TwoTanks_Equalize_WhenOneIsElevated()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            Vector3 gravity = new Vector3( 0, -10, 0 );

            // Setup Tank A (High Position)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 0.5f, 0 ) );
            tankA.Contents.Add( _water, 500 ); // Half full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1f, 0 ) + new Vector3( 0, 0.5f, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1f, 0 ) + new Vector3( 0, 0.5f, 0 ) )
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
            Assert.That( tankA.Contents.GetMass(), Is.GreaterThanOrEqualTo( 0 ) ); // check if flow works correctly and no negatives.
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThanOrEqualTo( 0 ) );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 500 ).Within( 1e-3 ) ); // check if mass doesn't get created out of thin air.
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( tankA.Contents.GetMass() ), "Fluid should have flowed to the lower tank (B) due to gravity." );
        }

        [UnityTest]
        public IEnumerator PipeAtTop_DoesNotDrain_WhenFluidLevelIsLow()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            // Setup Tank A (Source) - High up
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 10, 0 ) );
            tankA.Contents.Add( _water, 100 ); // Only 10% full (Level is at bottom of tank)

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(0, 1f, 0) + new Vector3( 0, 10, 0 )), // Index 0: Top
                new ResourceInlet(1, new Vector3(0, -1f, 0) + new Vector3( 0, 10, 0 )) // Index 1: Bottom
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

        [UnityTest]
        public IEnumerator TwoPipes_ShareSource_DoNotDuplicateMass()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            Vector3 gravity = new Vector3( 0, -10, 0 );

            // --- Setup Source Tank ---
            var goSource = new GameObject( "SourceTank" );
            goSource.transform.SetParent( _root.transform );

            var tankSource = new FlowTank( 1.0 );
            Vector3 offset = new Vector3( 0, 10, 0 );
            var nodes = new[]
            {
                new Vector3( 0, 1, 0 ) + offset,
                new Vector3( 0, -1, 0 ) + offset,
                new Vector3( 1, 0, 0 ) + offset,
                new Vector3( -1, 0, 0 ) + offset,
                new Vector3( 0, 0, 1 ) + offset,
                new Vector3( 0, 0, -1 ) + offset
            };
            var inletsSource = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1, 0 ) + offset ), // Top
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) + offset ), // Bottom 1
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) + offset )  // Bottom 2 (Same position)
            };
            tankSource.SetNodes( nodes, inletsSource );
            tankSource.FluidAcceleration = gravity;
            tankSource.FluidState = new FluidState( 101325, 293.15, 0 );

            // Start with limited mass
            double initialMass = 20.0;
            tankSource.Contents.Add( _water, initialMass );

            var wrapperSource = goSource.AddComponent<MockFlowTankWrapper>();
            wrapperSource.Tank = tankSource;
            wrapperSource.Inlets = inletsSource;

            // --- Setup Sink Tanks ---
            var goSink1 = new GameObject( "SinkTank1" );
            goSink1.transform.SetParent( _root.transform );
            var tankSink1 = CreateTestTank( 1.0, gravity, Vector3.zero );
            var wrapperSink1 = goSink1.AddComponent<MockFlowTankWrapper>();
            wrapperSink1.Tank = tankSink1;
            wrapperSink1.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, 1, 0 ) ), new ResourceInlet( 1, new Vector3( 0, -1, 0 ) ) };

            var goSink2 = new GameObject( "SinkTank2" );
            goSink2.transform.SetParent( _root.transform );
            var tankSink2 = CreateTestTank( 1.0, gravity, new Vector3( 2, 0, 0 ) );
            var wrapperSink2 = goSink2.AddComponent<MockFlowTankWrapper>();
            wrapperSink2.Tank = tankSink2;
            wrapperSink2.Inlets = new[] { new ResourceInlet( 1, new Vector3( 2, 1, 0 ) ), new ResourceInlet( 1, new Vector3( 2, -1, 0 ) ) };

            // --- Connect Pipes ---
            var pipe1 = _root.AddComponent<MockFlowPipeWrapper>();
            pipe1.FromInlet = wrapperSource.Inlets[1];
            pipe1.ToInlet = wrapperSink1.Inlets[0];

            var pipe2_GO = new GameObject( "Pipe2" );
            pipe2_GO.transform.SetParent( _root.transform );
            var pipe2 = pipe2_GO.AddComponent<MockFlowPipeWrapper>();
            pipe2.FromInlet = wrapperSource.Inlets[2];
            pipe2.ToInlet = wrapperSink2.Inlets[0];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            float simulationTime = 1.0f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
            }

            // Assert
            double massSource = tankSource.Contents.GetMass();
            double massSink1 = tankSink1.Contents.GetMass();
            double massSink2 = tankSink2.Contents.GetMass();
            double totalMass = massSource + massSink1 + massSink2;

            Debug.Log( $"Source: {massSource}, Sink1: {massSink1}, Sink2: {massSink2}, Total: {totalMass}" );

            // 1. Check that flow actually happened (Source is not full, Sinks are not empty)
            Assert.That( massSource, Is.LessThan( initialMass ), "Source should have lost some mass." );
            Assert.That( massSink1, Is.GreaterThan( 0 ), "Sink 1 should have received fluid." );
            Assert.That( massSink2, Is.GreaterThan( 0 ), "Sink 2 should have received fluid." );

            // 2. THE CRITICAL CHECK: Mass Conservation
            // The sum of all tanks must equal the initial mass (within small float epsilon).
            // If the bug exists, TotalMass will be > initialMass.
            Assert.AreEqual( initialMass, totalMass, 0.001, "Total mass was not conserved! Mass creation occurred." );
        }

        [UnityTest]
        public IEnumerator DaisyChain_FlowsThroughMiddleTank()
        {
            // Tests A -> B -> C configuration.
            // B acts as both a Sink (for A) and a Source (for C).

            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );
            Vector3 gravity = new Vector3( 0, -10, 0 );

            // Tank A: Top (Full)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 10, 0 ) );
            tankA.Contents.Add( _water, 1000 );
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 11, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, 9, 0 ) )
            };

            // Tank B: Middle (Empty initially)
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 5, 5, 0 ) );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 5, 6, 0 ) ), // Top Inlet
                new ResourceInlet( 1, new Vector3( 5, 4, 0 ) )  // Bottom Inlet
            };

            // Tank C: Bottom (Empty initially)
            var goC = new GameObject( "TankC" );
            goC.transform.SetParent( _root.transform );
            var tankC = CreateTestTank( 1.0, gravity, Vector3.zero );
            var wrapperC = goC.AddComponent<MockFlowTankWrapper>();
            wrapperC.Tank = tankC;
            wrapperC.Inlets = new[]
            {
                new ResourceInlet( 1, new Vector3( 0, 1, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) )
            };

            // Pipe 1: A (Bottom) -> B (Top)
            var pipe1 = _root.AddComponent<MockFlowPipeWrapper>();
            pipe1.FromInlet = wrapperA.Inlets[1];
            pipe1.ToInlet = wrapperB.Inlets[0];

            // Pipe 2: B (Bottom) -> C (Top)
            var pipe2_GO = new GameObject( "Pipe2" );
            pipe2_GO.transform.SetParent( _root.transform );
            var pipe2 = pipe2_GO.AddComponent<MockFlowPipeWrapper>();
            pipe2.FromInlet = wrapperB.Inlets[1];
            pipe2.ToInlet = wrapperC.Inlets[0];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            // Run long enough for fluid to traverse A->B->C
            float simulationTime = 5.0f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            double massC = tankC.Contents.GetMass();

            Assert.Less( massA, 999, "Tank A should drain." );
            Assert.Greater( massB, 0, "Tank B should contain some fluid in transit." );
            Assert.Greater( massC, 0, "Tank C should receive fluid from B." );

            Assert.AreEqual( 1000.0, massA + massB + massC, 1.0, "Total mass conservation failed." );
        }



        [UnityTest]
        public IEnumerator Gas_FlowsFromHighToLowPressure_Horizontal()
        {
            // Gasses should flow based on pressure differentials, equalizing even without gravity assistance.

            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            // Setup Tank A (High Pressure Source)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, Vector3.zero, new Vector3( -2, 0, 0 ) );
            // Add significant air mass to create high pressure
            tankA.Contents.Add( _air, 5.0 );

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(-2, 1, 0)),
                new ResourceInlet(1, new Vector3(-2, -1, 0))
            };

            // Setup Tank B (Low Pressure / Vacuum)
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, Vector3.zero, new Vector3( 2, 0, 0 ) );
            // Tank B is empty (0 pressure)

            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(2, 1, 0)),
                new ResourceInlet(1, new Vector3(2, -1, 0))
            };

            // Connect A to B
            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[0];
            pipe.ToInlet = wrapperB.Inlets[0];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            float simulationTime = 5.0f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            // Since tanks are identical volume and horizontal, they should equalize at ~50% mass each
            Assert.AreEqual( 2.5, tankA.Contents.GetMass(), 0.1, "Source tank did not depressurize correctly." );
            Assert.AreEqual( 2.5, tankB.Contents.GetMass(), 0.1, "Destination tank did not pressurize correctly." );
        }

        [UnityTest]
        public IEnumerator Gas_ExpandsToFillContainer_FlowsOutTopPipe()
        {
            // Contrast with 'PipeAtTop_DoesNotDrain_WhenFluidLevelIsLow'.
            // A small amount of gas should expand to fill the volume and exit via a TOP pipe,
            // whereas a liquid would settle at the bottom and not reach the pipe.

            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            // Setup Tank A (Source)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            // Add a SMALL amount of air. For a liquid, this would sit at the bottom.
            // For a gas, this fills the tank.
            double initialMass = 0.5;
            tankA.Contents.Add( _air, initialMass );

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(0, 1f, 0)), // Index 0: Top
                new ResourceInlet(1, new Vector3(0, -1f, 0)) // Index 1: Bottom
            };

            // Setup Tank B (Sink)
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 2, 0, 0 ) );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(2, 1f, 0)),
                new ResourceInlet(1, new Vector3(2, -1f, 0))
            };

            // Connect TOP of A to Top of B
            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[0]; // Connecting to Top inlet
            pipe.ToInlet = wrapperB.Inlets[0];

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
            Assert.Less( tankA.Contents.GetMass(), initialMass, "Gas should have escaped through the top pipe." );
            Assert.Greater( tankB.Contents.GetMass(), 0.0, "Gas should have entered the second tank." );
            Assert.AreEqual( 0.5, tankB.Contents.GetMass() + tankA.Contents.GetMass(), 0.01, "Total mass conservation failed." );
        }

        [UnityTest]
        public IEnumerator Gas_FlowsUpwards_AgainstGravity()
        {
            // Unlike liquids, gas flow is dominated by pressure, not gravity head.
            // High pressure gas at the bottom should flow UP into a low pressure tank at the top.

            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );
            Vector3 gravity = new Vector3( 0, -10, 0 );

            // Tank A (Bottom, High Pressure)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, gravity, Vector3.zero );
            tankA.Contents.Add( _air, 10.0 ); // High pressure

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, 1, 0 ) ) };

            // Tank B (Top, Empty)
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 0, 20, 0 ) ); // High up

            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, 19, 0 ) ) }; // Inlet at bottom of top tank

            // Connect A to B
            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[0];
            pipe.ToInlet = wrapperB.Inlets[0];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            float simulationTime = 5.0f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.Greater( tankB.Contents.GetMass(), 1.0, "Gas failed to flow upwards against gravity." );
            Assert.Less( tankA.Contents.GetMass(), 9.0, "Gas failed to leave the bottom tank." );
            Assert.AreEqual( 10.0, tankB.Contents.GetMass() + tankA.Contents.GetMass(), 0.01, "Total mass conservation failed." );
        }

        [UnityTest]
        public IEnumerator Gas_PressureEqualization_UnevenVolumes()
        {
            // When two tanks of DIFFERENT volumes containing GAS are connected:
            // They should equalize Pressure, NOT Mass.
            // Tank A (Small) should end up with less mass than Tank B (Large).

            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            // Tank A (Small, 1.0 Volume)
            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, Vector3.zero, new Vector3( -2, 0, 0 ) );
            tankA.Contents.Add( _air, 6.0 ); // Start with all mass here

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] { new ResourceInlet( 1, new Vector3( -1, 0, 0 ) ) };

            // Tank B (Large, 2.0 Volume)
            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 2.0, Vector3.zero, new Vector3( 2, 0, 0 ) );
            // Empty initially

            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 1, 0, 0 ) ) };

            // Connect
            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[0];
            pipe.ToInlet = wrapperB.Inlets[0];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            // Give it plenty of time to fully equalize
            float simulationTime = 8.0f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( Time.fixedDeltaTime );
                yield return new WaitForFixedUpdate();
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();

            // Total mass = 6.0. 
            // Volume Ratio is 1:2. Total Volume = 3.
            // Expected Mass A = 1/3 of 6.0 = 2.0
            // Expected Mass B = 2/3 of 6.0 = 4.0

            Assert.AreEqual( 2.0, massA, 0.2, "Small tank did not settle at correct proportional mass." );
            Assert.AreEqual( 4.0, massB, 0.2, "Large tank did not settle at correct proportional mass." );
            Assert.Greater( massB, massA, "Mass should not be equal; it should be proportional to volume for gasses." );
            Assert.AreEqual( 6.0, tankB.Contents.GetMass() + tankA.Contents.GetMass(), 0.01, "Total mass conservation failed." );
        }

        [UnityTest]
        public IEnumerator PhaseSeparation_GasLeavesTop_LiquidLeavesBottom()
        {
            // Verify that if a tank contains both Liquid and Gas:
            // 1. A pipe at the Top drains Gas first.
            // 2. A pipe at the Bottom drains Liquid first.

            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );
            Vector3 gravity = new Vector3( 0, -10, 0 );

            // -- Setup Tank A (Source: Mixed Water and Air) --
            var goA = new GameObject( "TankA_Mixed" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 2.0, gravity, Vector3.zero );
            tankA.Contents.Add( _water, 500 ); // Heavy liquid
            tankA.Contents.Add( _air, 10 );    // Light gas

            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[]
            {
                new ResourceInlet(1, new Vector3(0, 1, 0)), // Top
                new ResourceInlet(1, new Vector3(0, -1, 0)) // Bottom
            };

            // -- Setup Tank B (Top Receiver - Should get Gas) --
            var goB = new GameObject( "TankB_Gas" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 2, 2, 0 ) );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 2, 2, 0 ) ) };

            // -- Setup Tank C (Bottom Receiver - Should get Water) --
            var goC = new GameObject( "TankC_Liquid" );
            goC.transform.SetParent( _root.transform );
            var tankC = CreateTestTank( 1.0, gravity, new Vector3( -2, -2, 0 ) );
            var wrapperC = goC.AddComponent<MockFlowTankWrapper>();
            wrapperC.Tank = tankC;
            wrapperC.Inlets = new[] { new ResourceInlet( 1, new Vector3( -2, -2, 0 ) ) };

            // Connect A(Top) -> B
            var pipeGas = _root.AddComponent<MockFlowPipeWrapper>();
            pipeGas.FromInlet = wrapperA.Inlets[0];
            pipeGas.ToInlet = wrapperB.Inlets[0];

            // Connect A(Bottom) -> C
            var pipeLiq = goC.AddComponent<MockFlowPipeWrapper>(); // Attach to GO just to separate components
            pipeLiq.FromInlet = wrapperA.Inlets[1];
            pipeLiq.ToInlet = wrapperC.Inlets[0];

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            // Run a short step just to start flow
            snapshot.Step( Time.fixedDeltaTime * 10 );

            // Assert
            double waterInB = tankB.Contents[_water];
            double airInB = tankB.Contents[_air];

            double waterInC = tankC.Contents[_water];
            double airInC = tankC.Contents[_air];

            // Top pipe (B) should prefer Air
            Assert.Greater( airInB, 0, "Top pipe should transport gas." );
            Assert.AreEqual( 0, waterInB, 0.01, "Top pipe should NOT transport liquid while gas is present/liquid is low." );

            // Bottom pipe (C) should prefer Water
            Assert.Greater( waterInC, 0, "Bottom pipe should transport liquid." );
            // Note: Depending on simulation implementation, tiny amounts of air might be entrained, 
            // but usually bottom drains are pure liquid until empty.
            Assert.Less( airInC, 0.1, "Bottom pipe should generally exclude gas while liquid exists." );
        }


        [UnityTest]
        public IEnumerator Solver_HandlesOscillationProneSystem_WithoutException()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;

            _root = new GameObject( "TestRoot" );

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 100.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _water, 90000 ); // 90% full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] {
                new ResourceInlet(1, new Vector3(0, 2.32f, 0)),
                new ResourceInlet(1, new Vector3(0, -2.32f, 0))
            };

            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 100.0, new Vector3( 0, -10, 0 ), new Vector3( 10, 0, 0 ) );
            tankB.Contents.Add( _water, 10000 ); // 10% full
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] {
                new ResourceInlet(1, new Vector3(10, 2.32f, 0)),
                new ResourceInlet(1, new Vector3(10, -2.32f, 0))
            };

            var pipeGO = new GameObject( "Pipe" );
            pipeGO.transform.SetParent( _root.transform );
            var pipe = pipeGO.AddComponent<HighConductancePipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[1];
            pipe.ToInlet = wrapperB.Inlets[1];

            yield return new WaitForFixedUpdate();

            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );

            var s = snapshot;

            // Act & Assert
            float simulationTime = 5f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);

            for( int i = 0; i < steps; i++ )
            {
                Assert.DoesNotThrow( () => snapshot.Step( Time.fixedDeltaTime ), $"Solver threw a convergence exception at step {i}." );
                yield return new WaitForFixedUpdate();
            }

            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();

            Assert.AreEqual( 50000.0, massA, 15.0, "Tank A should have equalized." );
            Assert.AreEqual( 50000.0, massB, 15.0, "Tank B should have equalized." );
        }

        [UnityTest]
        public IEnumerator Solver_HandlesClosedLoop_WithoutException()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            // Create three tanks at the same height (y=0) in a triangle on the XZ plane.
            // All tanks are identical and half-full, creating a perfect equilibrium state.
            var goA = new GameObject( "TankA" ); goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( -5, 0, 0 ) );
            tankA.Contents.Add( _water, 600 ); // Half full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] { new ResourceInlet( 1, new Vector3( -5, 1.07f, 0 ) ), new ResourceInlet( 1, new Vector3( -5, -1.07f, 0 ) ) };

            var goB = new GameObject( "TankB" ); goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 5, 0, 0 ) );
            tankB.Contents.Add( _water, 500 ); // Half full
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 5, 1.07f, 0 ) ), new ResourceInlet( 1, new Vector3( 5, -1.07f, 0 ) ) };

            var goC = new GameObject( "TankC" ); goC.transform.SetParent( _root.transform );
            var tankC = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 0, 5 ) );
            tankC.Contents.Add( _water, 400 ); // Half full
            var wrapperC = goC.AddComponent<MockFlowTankWrapper>();
            wrapperC.Tank = tankC;
            wrapperC.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, 1.07f, 5 ) ), new ResourceInlet( 1, new Vector3( 0, -1.07f, 5 ) ) };

            // Connect the bottom inlets in a ring: A -> B -> C -> A
            var pipeAB_GO = new GameObject( "Pipe_AB" ); pipeAB_GO.transform.SetParent( _root.transform );
            var pipeAB = pipeAB_GO.AddComponent<MockFlowPipeWrapper>();
            pipeAB.FromInlet = wrapperA.Inlets[1];
            pipeAB.ToInlet = wrapperB.Inlets[1];

            var pipeBC_GO = new GameObject( "Pipe_BC" ); pipeBC_GO.transform.SetParent( _root.transform );
            var pipeBC = pipeBC_GO.AddComponent<MockFlowPipeWrapper>();
            pipeBC.FromInlet = wrapperB.Inlets[1];
            pipeBC.ToInlet = wrapperC.Inlets[1];

            var pipeCA_GO = new GameObject( "Pipe_CA" ); pipeCA_GO.transform.SetParent( _root.transform );
            var pipeCA = pipeCA_GO.AddComponent<MockFlowPipeWrapper>();
            pipeCA.FromInlet = wrapperC.Inlets[1];
            pipeCA.ToInlet = wrapperA.Inlets[1];

            yield return new WaitForFixedUpdate();

            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );

            // Act & Assert
            // Run for a few steps and verify that the solver doesn't throw an exception
            // and that no flow occurs, as the system is in perfect equilibrium.
            float simulationTime = 10f;
            int steps = (int)(simulationTime / Time.fixedDeltaTime);

            for( int i = 0; i < steps; i++ )
            {
                Assert.DoesNotThrow( () => snapshot.Step( Time.fixedDeltaTime ), $"Solver threw an exception on step {i} with a closed loop." );
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual( 500.0, tankA.Contents.GetMass(), 0.1, "Tank A mass should not change in equilibrium." );
            Assert.AreEqual( 500.0, tankB.Contents.GetMass(), 0.1, "Tank B mass should not change in equilibrium." );
            Assert.AreEqual( 500.0, tankC.Contents.GetMass(), 0.1, "Tank C mass should not change in equilibrium." );
        }

        [UnityTest]
        public IEnumerator GenericConsumer_DrainsTank()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _fuel, 820 ); // Full of fuel
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, -1, 0 ) ) };

            var goEngine = new GameObject( "Engine" );
            goEngine.transform.SetParent( _root.transform );
            var wrapperEngine = goEngine.AddComponent<MockEngineWrapper>();
            wrapperEngine.Inlet = new ResourceInlet( 1, new Vector3( 0, -2, 0 ) );

            var pipe = _root.AddComponent<MockFlowPipeWrapper>();
            pipe.FromInlet = wrapperA.Inlets[0]; // bottom of tank
            pipe.ToInlet = wrapperEngine.Inlet;

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            snapshot.Step( Time.fixedDeltaTime * 10 );

            // Assert
            Assert.Less( tankA.Contents.GetMass(), 820.0, "Tank should have drained some fuel." );
        }

        [UnityTest]
        public IEnumerator Pump_MovesFluidUphill()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, gravity, Vector3.zero );
            tankA.Contents.Add( _water, 500 ); // Half full
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] {
                new ResourceInlet( 1, new Vector3( 0, 1, 0 ) ),
                new ResourceInlet( 1, new Vector3( 0, -1, 0 ) )
            };

            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, 4, 0 ) ) };

            var pump = _root.AddComponent<MockPumpWrapper>();
            pump.FromInlet = wrapperA.Inlets[1];
            pump.ToInlet = wrapperB.Inlets[0];
            pump.HeadAdded = 200.0; // J/kg. g*h = 10 * (4-1) = 30. 200 is plenty.

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            snapshot.Step( Time.fixedDeltaTime * 10 );

            // Assert
            Assert.Less( tankA.Contents.GetMass(), 500.0, "Pump should have moved fluid out of the lower tank." );
            Assert.Greater( tankB.Contents.GetMass(), 0.0, "Pump should have moved fluid into the upper tank." );
            Assert.AreEqual( 500.0, tankA.Contents.GetMass() + tankB.Contents.GetMass(), 1.0, "Mass should be conserved." );
        }

        [UnityTest]
        public IEnumerator Valve_WhenClosed_StopsFlow()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _water, 1000 );
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, -1, 0 ) ) };

            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 5, 0, 0 ) );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 5, -1, 0 ) ) };

            var valve = _root.AddComponent<MockValveWrapper>();
            valve.FromInlet = wrapperA.Inlets[0];
            valve.ToInlet = wrapperB.Inlets[0];
            valve.IsOpen = false;

            yield return new WaitForFixedUpdate();

            // Act
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            snapshot.Step( Time.fixedDeltaTime * 10 );

            // Assert
            Assert.AreEqual( 1000.0, tankA.Contents.GetMass(), 0.1, "Mass should not leave tank A when valve is closed." );
            Assert.AreEqual( 0.0, tankB.Contents.GetMass(), 0.1, "Mass should not enter tank B when valve is closed." );
        }

        [UnityTest]
        public IEnumerator Valve_WhenOpened_AllowsFlowAndEqualization()
        {
            // Arrange
            var (manager, _, _) = FlowNetworkTestHelper.CreateTestScene();
            _manager = manager;
            _root = new GameObject( "TestRoot" );

            var goA = new GameObject( "TankA" );
            goA.transform.SetParent( _root.transform );
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _water, 1000 );
            var wrapperA = goA.AddComponent<MockFlowTankWrapper>();
            wrapperA.Tank = tankA;
            wrapperA.Inlets = new[] { new ResourceInlet( 1, new Vector3( 0, -1, 0 ) ) };

            var goB = new GameObject( "TankB" );
            goB.transform.SetParent( _root.transform );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 5, 0, 0 ) );
            var wrapperB = goB.AddComponent<MockFlowTankWrapper>();
            wrapperB.Tank = tankB;
            wrapperB.Inlets = new[] { new ResourceInlet( 1, new Vector3( 5, -1, 0 ) ) };

            var valve = _root.AddComponent<MockValveWrapper>();
            valve.FromInlet = wrapperA.Inlets[0];
            valve.ToInlet = wrapperB.Inlets[0];
            valve.IsOpen = false;

            yield return new WaitForFixedUpdate();

            // Act 1: Valve is closed
            var snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root );
            snapshot.Step( Time.fixedDeltaTime * 10 );

            // Assert 1: No flow
            Assert.AreEqual( 1000.0, tankA.Contents.GetMass(), 0.1, "No flow should occur before valve is opened." );
            Assert.AreEqual( 0.0, tankB.Contents.GetMass(), 0.1, "No flow should occur before valve is opened." );

            // Act 2: Open valve and rebuild network
            valve.IsOpen = true;
            snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _root ); // Rebuilds the network with the open valve
            snapshot.Step( Time.fixedDeltaTime * 10 );

            // Assert 2: Flow has started
            Assert.Less( tankA.Contents.GetMass(), 1000.0, "Flow should start after valve is opened." );
            Assert.Greater( tankB.Contents.GetMass(), 0.0, "Flow should start after valve is opened." );
            Assert.AreEqual( 1000.0, tankA.Contents.GetMass() + tankB.Contents.GetMass(), 1.0, "Mass should be conserved after opening valve." );
        }
    }
}