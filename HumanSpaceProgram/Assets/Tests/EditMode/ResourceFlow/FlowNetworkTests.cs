using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkTests
    {
        private ISubstance _water;
        private ISubstance _air;
        private ISubstance _fuel;

        [SetUp]
        public void SetUp()
        {
            _water = new Substance( "water" )
            {
                DisplayName = "Water",
                Phase = SubstancePhase.Liquid,
                ReferenceDensity = 1000.0,
                ReferencePressure = 101325.0,
                BulkModulus = 2.2e9,
                DisplayColor = Color.blue
            };
            _air = new Substance( "air" )
            {
                DisplayName = "Air",
                Phase = SubstancePhase.Gas,
                MolarMass = 0.0289647,
                DisplayColor = Color.clear
            };
            _fuel = new Substance( "kerosene" )
            {
                DisplayName = "Rocket Fuel",
                Phase = SubstancePhase.Liquid,
                ReferenceDensity = 820.0,
                ReferencePressure = 101325.0,
                BulkModulus = 1.6e9,
                DisplayColor = Color.yellow
            };
        }

        public static FlowTank CreateTestTank( double volume, Vector3 acceleration, Vector3 offset )
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

        private static FlowPipe CreateAndAddPipe( FlowNetworkBuilder builder, IResourceConsumer from, Vector3 fromLocation, IResourceConsumer to, Vector3 toLocation, double conductance, float area = 0.1f )
        {
            var portA = new FlowPipe.Port( from, fromLocation, area );
            var portB = new FlowPipe.Port( to, toLocation, area );
            var pipe = new FlowPipe( portA, portB, conductance );
            builder.TryAddFlowObj( new object(), pipe );
            return pipe;
        }

        [Test]
        public void LiquidFlow___TwoIdenticalHorizontalTanks___LevelsEqualize()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();

            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( _water, 1000 ); // Full

            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            CreateAndAddPipe( builder, tankA, new Vector3( 0, -1f, 0 ), tankB, new Vector3( 0, -1f, 0 ), 0.0005 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            float simulationTime = 10f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                double massA1 = tankA.Contents.GetMass();
                double massB1 = tankB.Contents.GetMass();
                Debug.Log( massA1 + " : " + massB1 );
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            Assert.That( massA, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank A should have half the mass." );
            Assert.That( massB, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank B should have half the mass." );
            Assert.That( massA + massB, Is.EqualTo( 1000.0 ).Within( 1e-9 ), "Mass should be conserved." );
        }

        [Test]
        public void LiquidFlow___ElevatedToLowTank___LevelsSeekEquilibrium()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) );
            tankA.Contents.Add( _water, 500 ); // Half full

            var tankB = CreateTestTank( 1.0, gravity, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            CreateAndAddPipe( builder, tankA, new Vector3( 0, 4, 0 ), tankB, new Vector3( 0, -1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            float simulationTime = 10f;
            int steps = (int)(simulationTime / fixedDeltaTime);

            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.GreaterThanOrEqualTo( 0 ) );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThanOrEqualTo( 0 ) );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 500 ).Within( 1e-3 ) );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( tankA.Contents.GetMass() ), "Fluid should have flowed to the lower tank (B) due to gravity." );
        }

        [Test]
        public void LiquidFlow___ElevatedToLowTank_BothPrimed___LevelsSeekEquilibrium()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) ); // Elevated
            tankA.Contents.Add( _water, 500 ); // Half full

            var tankB = CreateTestTank( 1.0, gravity, Vector3.zero ); // Lower
            tankB.Contents.Add( _water, 250 ); // Quarter full

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            CreateAndAddPipe( builder, tankA, new Vector3( 0, 4, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 750 ).Within( 1e-3 ), "Total mass must be conserved." );
            Assert.That( tankA.Contents.GetMass(), Is.LessThan( 500 ), "Elevated tank A should have lost mass." );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 250 ), "Lower tank B should have gained mass." );
            // The final state will not be equal masses due to the height difference creating a permanent potential difference.
        }

        [Test]
        public void LiquidFlow___OutletAboveFluidSurface___DoesNotFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 10, 0 ) );
            tankA.Contents.Add( _water, 100 ); // Only 10% full

            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            // Connect TOP of A to Bottom of B
            CreateAndAddPipe( builder, tankA, new Vector3( 0, 11, 0 ), tankB, new Vector3( 0, -1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            float simulationTime = 5f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 100.0 ).Within( 0.1 ), "Tank A should not lose mass." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Tank B should not gain mass." );
        }

        [Test]
        public void LiquidFlow___OneSourceToTwoSinks___MassIsConserved()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankSource = CreateTestTank( 1.0, gravity, new Vector3( 0, 10, 0 ) );
            double initialMass = 20.0;
            tankSource.Contents.Add( _water, initialMass );

            var tankSink1 = CreateTestTank( 1.0, gravity, Vector3.zero );
            var tankSink2 = CreateTestTank( 1.0, gravity, new Vector3( 2, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankSource );
            builder.TryAddFlowObj( new object(), tankSink1 );
            builder.TryAddFlowObj( new object(), tankSink2 );

            CreateAndAddPipe( builder, tankSource, new Vector3( 0, 9, 0 ), tankSink1, new Vector3( 0, 1, 0 ), 1.0 );
            CreateAndAddPipe( builder, tankSource, new Vector3( 0, 9, 0 ), tankSink2, new Vector3( 2, 1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            float simulationTime = 1.0f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double massSource = tankSource.Contents.GetMass();
            double massSink1 = tankSink1.Contents.GetMass();
            double massSink2 = tankSink2.Contents.GetMass();
            double totalMass = massSource + massSink1 + massSink2;

            Assert.That( massSource, Is.LessThan( initialMass ), "Source should have lost some mass." );
            Assert.That( massSink1, Is.GreaterThan( 0 ), "Sink 1 should have received fluid." );
            Assert.That( massSink2, Is.GreaterThan( 0 ), "Sink 2 should have received fluid." );
            Assert.That( totalMass, Is.EqualTo( initialMass ).Within( 0.001 ), "Total mass was not conserved! Mass creation occurred." );
        }

        [Test]
        public void LiquidFlow___ThreeTanksInSeries___FlowsThroughIntermediateTank()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 10, 0 ) );
            tankA.Contents.Add( _water, 1000 );

            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 5, 5, 0 ) );
            var tankC = CreateTestTank( 1.0, gravity, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            CreateAndAddPipe( builder, tankA, new Vector3( 0, 9, 0 ), tankB, new Vector3( 5, 6, 0 ), 1.0 );
            CreateAndAddPipe( builder, tankB, new Vector3( 5, 4, 0 ), tankC, new Vector3( 0, 1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            for( int i = 0; i < 4; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            double massC = tankC.Contents.GetMass();

            Assert.That( massA + massB + massC, Is.EqualTo( 1000.0 ).Within( 1.0 ), "Total mass conservation failed." );
            Assert.That( massA, Is.LessThan( 999 ), "Tank A should drain." );
            Assert.That( massC, Is.GreaterThan( 0 ), "Tank C should receive fluid from B." );
            Assert.That( massB, Is.GreaterThan( 0 ), "Tank B should contain some fluid in transit." );
        }

        [Test]
        public void GasFlow___HighPressureToLowPressure_Horizontal___PressuresEqualizeByVolume()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankA = CreateTestTank( 1.0, gravity, new Vector3( -2, 0, 0 ) );
            tankA.Contents.Add( _air, 6.0 );

            var tankB = CreateTestTank( 2.0, gravity, new Vector3( 2, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            CreateAndAddPipe( builder, tankA, new Vector3( -1, 0, 0 ), tankB, new Vector3( 1, 0, 0 ), 0.001 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            float simulationTime = 8.0f;
            int steps = (int)(simulationTime / fixedDeltaTime);
            for( int i = 0; i < steps; i++ )
            {
                tankA.FluidState = new FluidState( pressure: VaporLiquidEquilibrium.ComputePressureOnly( tankA.Contents, tankA.FluidState, tankA.Volume ), temperature: tankA.FluidState.Temperature, tankA.FluidState.Velocity );
                tankB.FluidState = new FluidState( pressure: VaporLiquidEquilibrium.ComputePressureOnly( tankB.Contents, tankB.FluidState, tankB.Volume ), temperature: tankB.FluidState.Temperature, tankB.FluidState.Velocity );
                double massA1 = tankA.Contents.GetMass();
                double massB1 = tankB.Contents.GetMass();
                Debug.Log( massA1 + " : " + massB1 );
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double massA = tankA.Contents.GetMass();
            double massB = tankB.Contents.GetMass();
            Assert.That( massA + massB, Is.EqualTo( 6.0 ).Within( 0.01 ), "Total mass conservation failed." );
            Assert.That( massA, Is.EqualTo( 2.0 ).Within( 0.2 ), "Small tank did not settle at correct proportional mass." );
            Assert.That( massB, Is.EqualTo( 4.0 ).Within( 0.2 ), "Large tank did not settle at correct proportional mass." );
        }

        [Test]
        public void GasFlow___HighPressureToLowPressure_Uphill___FlowsAgainstGravity()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankA = CreateTestTank( 1.0, gravity, Vector3.zero ); // Bottom tank
            tankA.Contents.Add( _air, 5.0 ); // High pressure

            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 0, 10, 0 ) ); // Top tank

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            CreateAndAddPipe( builder, tankA, new Vector3( 0, 1, 0 ), tankB, new Vector3( 0, 9, 0 ), 1.0 );
            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            for( int i = 0; i < 250; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 0.1 ), "Gas should have flowed upwards to Tank B." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 5.0 ).Within( 1e-3 ), "Total gas mass must be conserved." );
        }

        [Test]
        public void MultiPhaseFlow___StratifiedTank___GasFlowsFromTop_LiquidFromBottom()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankSource = CreateTestTank( 2.0, gravity, Vector3.zero );
            tankSource.Contents.Add( _water, 1000 ); // 1.0 m^3 of water (fills bottom half)
            tankSource.Contents.Add( _air, 1.2 );    // 1.0 m^3 of air (fills top half)

            var tankGasSink = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) );
            var tankLiquidSink = CreateTestTank( 1.0, gravity, new Vector3( 0, -5, 0 ) );

            builder.TryAddFlowObj( new object(), tankSource );
            builder.TryAddFlowObj( new object(), tankGasSink );
            builder.TryAddFlowObj( new object(), tankLiquidSink );

            // Pipe from top of source to gas sink
            CreateAndAddPipe( builder, tankSource, new Vector3( 0, 1, 0 ), tankGasSink, new Vector3( 0, 4, 0 ), 10.0 );
            // Pipe from bottom of source to liquid sink
            CreateAndAddPipe( builder, tankSource, new Vector3( 0, -1, 0 ), tankLiquidSink, new Vector3( 0, -4, 0 ), 10.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            for( int i = 0; i < 50; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            Assert.That( tankGasSink.Contents.GetMass(), Is.GreaterThan( 0 ), "Gas sink should have received fluid." );
            Assert.That( tankGasSink.Contents.Contains( _air ), Is.True, "Gas sink should contain air." );
            Assert.That( tankGasSink.Contents.Contains( _water ), Is.False, "Gas sink should NOT contain water." );

            Assert.That( tankLiquidSink.Contents.GetMass(), Is.GreaterThan( 0 ), "Liquid sink should have received fluid." );
            Assert.That( tankLiquidSink.Contents.Contains( _water ), Is.True, "Liquid sink should contain water." );
            Assert.That( tankLiquidSink.Contents.Contains( _air ), Is.False, "Liquid sink should NOT contain air." );
        }

        [Test]
        public void Solver_HandlesOscillationProneSystem_WithoutException()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 0.1, gravity, Vector3.zero );
            tankA.Contents.Add( _water, 90 );

            var tankB = CreateTestTank( 0.1, gravity, Vector3.right );
            tankA.Contents.Add( _water, 10 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            // High conductance pipe
            CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 1, -1, 0 ), 1000.0 );
            var snapshot = builder.BuildSnapshot();

            // Act & Assert
            Assert.DoesNotThrow( () =>
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.Step( 0.02f );
                }
            }, "Solver threw an exception on an oscillation-prone system." );

            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 100.0 ).Within( 1e-3 ), "Mass must be conserved." );
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 50.0 ).Within( 1.0 ), "Tanks should equalize." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 50.0 ).Within( 1.0 ), "Tanks should equalize." );
        }

        [Test]
        public void Solver_HandlesClosedLoopLiquid_WithoutException()
        {
            static Vector3 GetPoint( int i, int count, float y )
            {
                return new Vector3(
                    Mathf.Cos( i * Mathf.PI / 2 ) * 5,
                    y,
                    Mathf.Sin( i * Mathf.PI / 2 ) * 5
                    );
            }

            // Arrange
            var builder = new FlowNetworkBuilder();
            var tanks = new List<FlowTank>();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            for( int i = 0; i < 4; i++ )
            {
                Vector3 pos = GetPoint( i, 4, 0 );
                var tank = CreateTestTank( 1.0, gravity, pos );
                tanks.Add( tank );
                builder.TryAddFlowObj( new object(), tank );
            }
            tanks[0].Contents.Add( _water, 970 ); // Prime one tank
            tanks[1].Contents.Add( _water, 10 ); // Prime one tank
            tanks[2].Contents.Add( _water, 10 ); // Prime one tank
            tanks[3].Contents.Add( _water, 10 ); // Prime one tank

            for( int i = 0; i < 4; i++ )
            {
                int source = i;
                int target = (i + 1) % 4;
                var sourceTank = tanks[source];
                var targetTank = tanks[target];
                Vector3 posSource = GetPoint( source, 4, -1 );
                Vector3 posTarget = GetPoint( target, 4, -1 );
                CreateAndAddPipe( builder, sourceTank, posSource, targetTank, posTarget, 0.001 );
            }
            var snapshot = builder.BuildSnapshot();

            // Act & Assert
            Assert.DoesNotThrow( () =>
            {
                for( int i = 0; i < 50; i++ )
                {
                    Debug.Log( tanks[0].Contents.GetMass() + " : " + tanks[1].Contents.GetMass() + " : " + tanks[2].Contents.GetMass() + " : " + tanks[3].Contents.GetMass() );
                    snapshot.Step( 0.02f );
                }
            }, "Solver failed on a closed liquid loop." );

            Assert.That( tanks.Sum( t => t.Contents.GetMass() ), Is.EqualTo( 1000.0 ).Within( 1e-3 ) );
            Assert.That( tanks[0].Contents.GetMass(), Is.EqualTo( 250 ).Within( 5.0 ) );
            Assert.That( tanks[1].Contents.GetMass(), Is.EqualTo( 250 ).Within( 5.0 ) );
            Assert.That( tanks[2].Contents.GetMass(), Is.EqualTo( 250 ).Within( 5.0 ) );
            Assert.That( tanks[3].Contents.GetMass(), Is.EqualTo( 250 ).Within( 5.0 ) );
        }

        [Test]
        public void EngineAsConsumer_DrainsTank()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tank = CreateTestTank( 1.0, gravity, Vector3.zero );
            tank.Contents.Add( _fuel, 800 ); // Full

            var engine = new EngineFeedSystem( 0.01 )
            {
                IsOutflowEnabled = true,
                ChamberPressure = 1e5, // Low chamber pressure to encourage flow
                InjectorConductance = 0.1,
                Demand = 100.0 // Request a lot
            };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), engine );
            CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), engine, Vector3.zero, 1.0 );
            var snapshot = builder.BuildSnapshot();

            double initialMass = tank.Contents.GetMass();

            // Act
            snapshot.Step( 0.02f );

            // Assert
            Assert.That( tank.Contents.GetMass(), Is.LessThan( initialMass ), "Tank should have lost mass." );
            Assert.That( engine.MassConsumedLastStep, Is.GreaterThan( 0 ), "Engine should have consumed mass." );
        }

        [Test]
        public void Pump_MovesFluidUphill()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 1.0, gravity, Vector3.zero ); // Bottom tank
            tankA.Contents.Add( _water, 500 );

            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) ); // Top tank

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, 4, 0 ), 1.0 );

            // Gravitational potential difference is g*h = 10 * 10 = 100.
            // Add more head than that to move fluid up.
            pipe.HeadAdded = 200.0;

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.LessThan( 500 ), "Bottom tank should lose mass." );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 0 ), "Top tank should gain mass." );
        }

        [Test]
        public void Pump_MovesFluidDownhill()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 1.0, gravity, new Vector3( 0, 5, 0 ) ); // Top tank
            tankA.Contents.Add( _water, 500 );

            var tankB = CreateTestTank( 1.0, gravity, Vector3.zero ); // Bottom tank

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, 4, 0 ), 1.0 );

            // Gravitational potential difference is g*h = 10 * 10 = 100.
            // Add more head than that to move fluid up.
            pipe.HeadAdded = 200.0;

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );
            snapshot.Step( 0.02f );

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.LessThan( 500 ), "Bottom tank should lose mass." );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 0 ), "Top tank should gain mass." );
        }

        [Test]
        public void LiquidFlow___ZeroConductancePipe___NoFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 10, 0 ) );
            tankA.Contents.Add( _water, 1000 );

            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, 9, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0 );

            pipe.Conductance = 0.0; // Simulate a closed valve

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( 0.02f );

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 1000.0 ), "Source tank mass should not change." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0.0 ), "Sink tank mass should not change." );
        }

        [Test]
        public void LiquidFlow___StrongFlowIntoSmallTank___DoesntOverfill()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 10.0, gravity, new Vector3( 0, 200, 0 ) );
            tankA.Contents.Add( _water, 10000 ); // Full

            var tankB = CreateTestTank( 0.1, gravity, Vector3.zero ); // Very small

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            CreateAndAddPipe( builder, tankA, new Vector3( 0, 199, 0 ), tankB, new Vector3( 0, 1, 0 ), 10.0 ); // High conductance

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            for( int i = 0; i < 25; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double volumeInTankB = tankB.Contents.GetVolume( tankB.FluidState.Temperature, tankB.FluidState.Pressure );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 10000.0 ).Within( 0.01 ), "Mass must be conserved." );
            Assert.That( volumeInTankB, Is.EqualTo( tankB.Volume ).Within( 1e-6 ), "Small tank should not be overfilled in a single step." );
        }

        [Test]
        public void LiquidFlow___StrongFlowFromSmallTank___DoesntUnderfill()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 10.0, gravity, Vector3.zero );

            var tankB = CreateTestTank( 1, gravity, new Vector3( 0, 200, 0 ) ); // Very small
            tankB.Contents.Add( _water, 1000 ); // Full

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            CreateAndAddPipe( builder, tankB, new Vector3( 0, 199, 0 ), tankA, new Vector3( 0, 1, 0 ), 10.0 ); // High conductance

            var snapshot = builder.BuildSnapshot();

            // Act
            const float fixedDeltaTime = 0.02f;
            for( int i = 0; i < 25; i++ )
            {
                snapshot.Step( fixedDeltaTime );
            }

            // Assert
            double volumeInTankA = tankA.Contents.GetVolume( tankA.FluidState.Temperature, tankA.FluidState.Pressure );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.01 ), "Mass must be conserved." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0 ), "Small tank should be drained and not underfilled." );
            Assert.That( volumeInTankA, Is.EqualTo( tankB.Volume ).Within( 1e-3 ), "Small tank should be drained and not underfilled." );
        }
    }
}