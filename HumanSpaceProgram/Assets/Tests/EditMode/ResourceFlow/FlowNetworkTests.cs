using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using HSP_Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkTests
    {
        private double[] GetLearnedRelaxationFactors( FlowNetworkSnapshot snapshot )
        {
            var fieldInfo = typeof( FlowNetworkSnapshot ).GetField( "_pipeLearnedRelaxationFactors", BindingFlags.NonPublic | BindingFlags.Instance );
            if( fieldInfo == null )
            {
                Assert.Fail( "_pipeLearnedRelaxationFactors field not found on FlowNetworkSnapshot. This may be due to a refactor." );
                return null;
            }
            return (double[])fieldInfo.GetValue( snapshot );
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
            tankA.Contents.Add( TestSubstances.Water, 1000 ); // Full

            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            CreateAndAddPipe( builder, tankA, new Vector3( 0, -1f, 0 ), tankB, new Vector3( 0, -1f, 0 ), 0.5 );

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
            tankA.Contents.Add( TestSubstances.Water, 500 ); // Half full

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
            tankA.Contents.Add( TestSubstances.Water, 500 ); // Half full

            var tankB = CreateTestTank( 1.0, gravity, Vector3.zero ); // Lower
            tankB.Contents.Add( TestSubstances.Water, 250 ); // Quarter full

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
            tankA.Contents.Add( TestSubstances.Water, 100 ); // Only 10% full

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
            tankSource.Contents.Add( TestSubstances.Water, initialMass );

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
            tankA.Contents.Add( TestSubstances.Water, 1000 );

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
            tankA.Contents.Add( TestSubstances.Air, 6.0 );

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
            tankA.Contents.Add( TestSubstances.Air, 5.0 ); // High pressure

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
            tankSource.Contents.Add( TestSubstances.Water, 1000 ); // 1.0 m^3 of water (fills bottom half)
            tankSource.Contents.Add( TestSubstances.Air, 1.2 );    // 1.0 m^3 of air (fills top half)

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
            Assert.That( tankGasSink.Contents.Contains( TestSubstances.Air ), Is.True, "Gas sink should contain air." );
            Assert.That( tankGasSink.Contents.Contains( TestSubstances.Water ), Is.False, "Gas sink should NOT contain water." );

            Assert.That( tankLiquidSink.Contents.GetMass(), Is.GreaterThan( 0 ), "Liquid sink should have received fluid." );
            Assert.That( tankLiquidSink.Contents.Contains( TestSubstances.Water ), Is.True, "Liquid sink should contain water." );
            Assert.That( tankLiquidSink.Contents.Contains( TestSubstances.Air ), Is.False, "Liquid sink should NOT contain air." );
        }

        [Test]
        public void Solver_HandlesOscillationProneSystem_WithoutException()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 0.1, gravity, Vector3.zero );
            tankA.Contents.Add( TestSubstances.Water, 90 );

            var tankB = CreateTestTank( 0.1, gravity, new Vector3( 1, 0, 0 ) );
            tankB.Contents.Add( TestSubstances.Water, 10 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            // High conductance pipe
            CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 1, -1, 0 ), 1000.0 );
            var snapshot = builder.BuildSnapshot();

            // Act & Assert
            Assert.DoesNotThrow( () =>
            {
                for( int i = 0; i < 50; i++ )
                {
                    Debug.Log( tankA.Contents.GetMass() + " : " + tankB.Contents.GetMass() );
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
            tanks[0].Contents.Add( TestSubstances.Water, 970 ); // Prime one tank
            tanks[1].Contents.Add( TestSubstances.Water, 10 ); // Prime one tank
            tanks[2].Contents.Add( TestSubstances.Water, 10 ); // Prime one tank
            tanks[3].Contents.Add( TestSubstances.Water, 10 ); // Prime one tank

            for( int i = 0; i < 4; i++ )
            {
                int source = i;
                int target = (i + 1) % 4;
                var sourceTank = tanks[source];
                var targetTank = tanks[target];
                Vector3 posSource = GetPoint( source, 4, -1 );
                Vector3 posTarget = GetPoint( target, 4, -1 );
                CreateAndAddPipe( builder, sourceTank, posSource, targetTank, posTarget, 0.5 );
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
            tank.Contents.Add( TestSubstances.Kerosene, 800 ); // Full

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
            tankA.Contents.Add( TestSubstances.Water, 500 );

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
            tankA.Contents.Add( TestSubstances.Water, 500 );

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
            tankA.Contents.Add( TestSubstances.Water, 1000 );

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
            tankA.Contents.Add( TestSubstances.Water, 10000 ); // Full

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
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 100 ).Within( 0.01 ), "Small tank should be drained and not underfilled." );
            Assert.That( volumeInTankB, Is.EqualTo( tankB.Volume ).Within( 1e-3 ), "Small tank should not be overfilled in a single step." );
        }

        [Test]
        public void LiquidFlow___StrongFlowFromSmallTank___DoesntUnderfill()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );
            var tankA = CreateTestTank( 10.0, gravity, Vector3.zero );

            var tankB = CreateTestTank( 1, gravity, new Vector3( 0, 200, 0 ) ); // Very small
            tankB.Contents.Add( TestSubstances.Water, 1000 ); // Full

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
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0 ).Within( 0.01 ), "Small tank should be drained and not underfilled." );
            Assert.That( volumeInTankA, Is.EqualTo( tankB.Volume ).Within( 1e-3 ), "Small tank should be drained and not underfilled." );
        }

        [Test]
        public void Solver_WithMixedStiffnessTanks_DampsStiffConnectionProactively()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankSource = CreateTestTank( 10.0, gravity, Vector3.zero );
            tankSource.Contents.Add( TestSubstances.Water, 10000 ); // Full

            var tankStiffSink = CreateTestTank( 0.1, gravity, new Vector3( -5, 0, 0 ) ); // Small, liquid-only -> stiff
            var tankGasSink = CreateTestTank( 5.0, gravity, new Vector3( 5, 0, 0 ) );   // Large, gas-only -> not stiff

            builder.TryAddFlowObj( new object(), tankSource );
            builder.TryAddFlowObj( new object(), tankStiffSink );
            builder.TryAddFlowObj( new object(), tankGasSink );

            // Identical pipes to isolate effect of stiffness
            var pipeToStiff = CreateAndAddPipe( builder, tankSource, new Vector3( 0, -1, 0 ), tankStiffSink, new Vector3( -4, 0, 0 ), 1.0 );
            var pipeToGas = CreateAndAddPipe( builder, tankSource, new Vector3( 0, -1, 0 ), tankGasSink, new Vector3( 4, 0, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act: Run a single step. Proactive damping should work on the first iteration.
            snapshot.Step( 0.02f );

            // Assert
            double massToStiff = tankStiffSink.Contents.GetMass();
            double massToGas = tankGasSink.Contents.GetMass();

            Assert.That( massToStiff, Is.GreaterThan( 0 ), "Some flow should occur to stiff tank." );
            Assert.That( massToGas, Is.GreaterThan( 0 ), "Some flow should occur to gas tank." );

            // Due to proactive damping, flow to the stiff tank should be significantly less than to the non-stiff one,
            // even though potential gradients are comparable.
            Assert.That( massToGas, Is.GreaterThan( massToStiff * 5 ), "Flow to non-stiff tank should be much higher than to stiff tank." );
        }

        [Test]
        public void Solver_WithMixedConductancePipes_LearnsToDampOscillatingPipe()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankA = CreateTestTank( 1.0, gravity, new Vector3( -10, 0, 0 ) );
            var tankB = CreateTestTank( 1.0, gravity, new Vector3( -5, 0, 0 ) );
            tankA.Contents.Add( TestSubstances.Water, 1000 );

            var tankC = CreateTestTank( 1.0, gravity, new Vector3( 5, 0, 0 ) );
            var tankD = CreateTestTank( 1.0, gravity, new Vector3( 10, 0, 0 ) );
            tankC.Contents.Add( TestSubstances.Water, 1000 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );
            builder.TryAddFlowObj( new object(), tankD );

            var stablePipe = CreateAndAddPipe( builder, tankA, new Vector3( -9, 0, 0 ), tankB, new Vector3( -6, 0, 0 ), 0.001 ); // Low conductance
            var unstablePipe = CreateAndAddPipe( builder, tankC, new Vector3( 6, 0, 0 ), tankD, new Vector3( 9, 0, 0 ), 1000.0 ); // High conductance

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 50; i++ )
            {
                snapshot.Step( 0.02f );
            }

            // Assert
            var learnedFactors = GetLearnedRelaxationFactors( snapshot );
            int stablePipeIndex = snapshot.Pipes.ToList().IndexOf( stablePipe );
            int unstablePipeIndex = snapshot.Pipes.ToList().IndexOf( unstablePipe );

            Assert.That( stablePipeIndex, Is.Not.EqualTo( -1 ) );
            Assert.That( unstablePipeIndex, Is.Not.EqualTo( -1 ) );

            Assert.That( learnedFactors[unstablePipeIndex], Is.LessThan( 0.5 ), "High-conductance pipe should have its learned damping factor significantly reduced." );
            Assert.That( learnedFactors[stablePipeIndex], Is.EqualTo( 1.0 ).Within( 0.1 ), "Low-conductance pipe should not be damped and its factor should recover to ~1.0." );
        }

        [Test]
        public void Solver_WithHighStiffnessAndConductance_ConvergesSafely()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankSource = CreateTestTank( 0.2, gravity, Vector3.zero );
            tankSource.Contents.Add( TestSubstances.Water, 200 ); // Full

            var tankStiffSink = CreateTestTank( 0.2, gravity, new Vector3( 5, 0, 0 ) ); // Stiff

            builder.TryAddFlowObj( new object(), tankSource );
            builder.TryAddFlowObj( new object(), tankStiffSink );

            // Worst-case: high conductance pipe into a stiff tank
            var pipe = CreateAndAddPipe( builder, tankSource, new Vector3( 1, 0, 0 ), tankStiffSink, new Vector3( 4, 0, 0 ), 1000.0 );

            var snapshot = builder.BuildSnapshot();

            // Act & Assert
            Assert.DoesNotThrow( () =>
            {
                for( int i = 0; i < 100; i++ )
                {
                    Debug.Log( tankSource.Contents.GetMass() + " : " + tankStiffSink.Contents.GetMass() );
                    snapshot.Step( 0.02f );
                }
            }, "Solver threw an exception on a highly stiff and conductive system." );

            double massA = tankSource.Contents.GetMass();
            double massB = tankStiffSink.Contents.GetMass();

            Assert.That( massA + massB, Is.EqualTo( 200.0 ).Within( 1e-3 ), "Mass must be conserved." );
            Assert.That( massA, Is.EqualTo( 100.0 ).Within( 1.0 ), "Tanks should equalize." );
            Assert.That( massB, Is.EqualTo( 100.0 ).Within( 1.0 ), "Tanks should equalize." );

            var learnedFactors = GetLearnedRelaxationFactors( snapshot );
            int pipeIndex = snapshot.Pipes.ToList().IndexOf( pipe );
            Assert.That( learnedFactors[pipeIndex], Is.LessThan( 0.1 ), "Pipe should be very aggressively damped by the reactive layer." );
        }

        // Additional tests for edge-cases and failure modes
        [Test]
        public void LiquidFlow___ZeroVolumeTank_HandledSafelyAndMassConserved()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankSource = CreateTestTank( 1.0, gravity, new Vector3( 0, 2, 0 ) );
            tankSource.Contents.Add( TestSubstances.Water, 1000 );

            // Create a zero-volume tank (edge-case)
            var tankZero = CreateTestTank( 0.0, gravity, Vector3.zero );
            // Even if volume is zero, API should not throw and mass should remain zero or be handled gracefully
            // Do not add mass to zero-volume tank.

            builder.TryAddFlowObj( new object(), tankSource );
            builder.TryAddFlowObj( new object(), tankZero );

            var pipe = CreateAndAddPipe( builder, tankSource, new Vector3( 0, 1, 0 ), tankZero, new Vector3( 0, -1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act & Assert: stepping should not throw and mass must be conserved.
            Assert.DoesNotThrow( () =>
            {
                for( int i = 0; i < 20; i++ )
                {
                    snapshot.Step( 0.02f );
                }
            }, "Solver threw with a zero-volume tank." );

            Assert.That( tankSource.Contents.GetMass() + tankZero.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Total mass must be conserved." );
            // Zero-volume tank should not contain any fluid mass (or must be capped by implementation).
            Assert.That( tankZero.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 1e-6 ), "Zero-volume tank must not gain mass." );
        }

        [Test]
        public void LiquidFlow___ValveToggledMidSimulation_StopsFlowAndConservesMass()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 0, 5, 0 ) );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );

            tankA.Contents.Add( TestSubstances.Water, 500 );
            tankB.Contents.Add( TestSubstances.Water, 0 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, 4, 0 ), tankB, new Vector3( 0, 1, 0 ), 5.0 );

            var snapshot = builder.BuildSnapshot();

            // Act: run a few steps with valve open
            for( int i = 0; i < 10; i++ ) snapshot.Step( 0.02f );

            double massAfterOpen = tankB.Contents.GetMass();

            // Toggle valve closed
            pipe.Conductance = 0.0;

            // Run more steps; no further flow should occur
            for( int i = 0; i < 20; i++ ) snapshot.Step( 0.02f );

            // Assert
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( massAfterOpen ).Within( 1e-6 ), "No additional mass should pass after valve closed." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 500.0 ).Within( 1e-6 ), "Total mass must be conserved." );
        }

        [Test]
        public void MultiSubstance___TwoSourcesDifferentSubstances_MixInSink()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var sourceA = CreateTestTank( 1.0, gravity, new Vector3( -2, 10, 0 ) );
            sourceA.Contents.Add( TestSubstances.Water, 500 );

            var sourceB = CreateTestTank( 1.0, gravity, new Vector3( 2, 10, 0 ) );
            sourceB.Contents.Add( TestSubstances.Kerosene, 250 );

            var sink = CreateTestTank( 2.0, gravity, Vector3.zero );

            builder.TryAddFlowObj( new object(), sourceA );
            builder.TryAddFlowObj( new object(), sourceB );
            builder.TryAddFlowObj( new object(), sink );

            CreateAndAddPipe( builder, sourceA, new Vector3( -2, 9, 0 ), sink, new Vector3( 0, 1, 0 ), 1.0 );
            CreateAndAddPipe( builder, sourceB, new Vector3( 2, 9, 0 ), sink, new Vector3( 0, 1, 0 ), 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act: run until flows transfer some mass
            for( int i = 0; i < 200; i++ ) snapshot.Step( 0.02f );

            // Assert: sink must contain both substances and global mass conserved
            Assert.That( sink.Contents.Contains( TestSubstances.Water ), Is.True, "Sink should contain water from source A." );
            Assert.That( sink.Contents.Contains( TestSubstances.Kerosene ), Is.True, "Sink should contain kerosene from source B." );

            double totalMass = sourceA.Contents.GetMass() + sourceB.Contents.GetMass() + sink.Contents.GetMass();
            Assert.That( totalMass, Is.EqualTo( 500 + 250 ).Within( 1e-6 ), "Total mass must be conserved across multiple substances." );
        }

        [Test]
        public void Pump_ReverseHead_BackAndForth_DoesNotProduceMassOrThrow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( -1, 0, 0 ) );
            var tankB = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 1, 0, 0 ) );

            tankA.Contents.Add( TestSubstances.Water, 600 );
            tankB.Contents.Add( TestSubstances.Water, 400 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, -1, 0 ), 10.0 );

            var snapshot = builder.BuildSnapshot();

            // Act: repeatedly change pump head (simulate quick reversal) and step
            Assert.DoesNotThrow( () =>
            {
                for( int cycle = 0; cycle < 20; cycle++ )
                {
                    // pump in direction A->B
                    pipe.HeadAdded = 200.0;
                    snapshot.Step( 0.02f );
                    // pump in direction B->A (negative head)
                    pipe.HeadAdded = -200.0;
                    snapshot.Step( 0.02f );
                    // neutral
                    pipe.HeadAdded = 0.0;
                    snapshot.Step( 0.02f );
                }
            }, "Solver threw when pump head reversed rapidly." );

            // Assert conservation of mass
            double total = tankA.Contents.GetMass() + tankB.Contents.GetMass();
            Assert.That( total, Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved under rapid pump reversals." );
        }

        [Test]
        public void NumericStability___TinyTimeStepRepeated_SumsToSameMass()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            Vector3 gravity = new Vector3( 0, -10, 0 );

            var tankA = CreateTestTank( 1.0, gravity, new Vector3( -1, 0, 0 ) );
            var tankB = CreateTestTank( 1.0, gravity, new Vector3( 1, 0, 0 ) );

            tankA.Contents.Add( TestSubstances.Air, 10.0 ); // gas
            tankB.Contents.Add( TestSubstances.Air, 0.0 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            CreateAndAddPipe( builder, tankA, new Vector3( 0, 0, 0 ), tankB, new Vector3( 0, 0, 0 ), 0.1 );

            var snapshot = builder.BuildSnapshot();

            // Act: many tiny steps vs one large step should not create/destroy mass
            int tinySteps = 1000;
            float tinyDt = 1e-5f;
            for( int i = 0; i < tinySteps; i++ )
            {
                snapshot.Step( tinyDt );
            }

            double totalAfterTiny = tankA.Contents.GetMass() + tankB.Contents.GetMass();

            // Reset and do single-step equivalent
            builder = new FlowNetworkBuilder();
            tankA = CreateTestTank( 1.0, gravity, new Vector3( -1, 0, 0 ) );
            tankB = CreateTestTank( 1.0, gravity, new Vector3( 1, 0, 0 ) );
            tankA.Contents.Add( TestSubstances.Air, 10.0 );
            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            CreateAndAddPipe( builder, tankA, new Vector3( 0, 0, 0 ), tankB, new Vector3( 0, 0, 0 ), 0.1 );
            var snapshot2 = builder.BuildSnapshot();

            snapshot2.Step( tinySteps * tinyDt );

            double totalAfterLarge = tankA.Contents.GetMass() + tankB.Contents.GetMass();

            Assert.That( totalAfterTiny, Is.EqualTo( totalAfterLarge ).Within( 1e-6 ), "Many tiny steps should conserve mass equivalently to one larger step." );
            Assert.That( totalAfterTiny, Is.EqualTo( 10.0 ).Within( 1e-6 ), "Total mass must be conserved after many tiny steps." );
        }

        [Test]
        public void EngineDemand_IsLimited_By_InjectorConductance()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tank = CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tank.Contents.Add( TestSubstances.Kerosene, 800 );

            var engine = new EngineFeedSystem( 0.01 )
            {
                IsOutflowEnabled = true,
                ChamberPressure = 1e5,
                InjectorConductance = 0.0001, // tiny injector conductance should cap flow
                Demand = 10000.0 // wildly large demand
            };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), engine );
            var pipe = CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), engine, Vector3.zero, 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( 0.02f );

            // Assert: engine must have consumed mass but should be limited by injector conductance (not consume entire tank)
            Assert.That( engine.MassConsumedLastStep, Is.GreaterThan( 0 ), "Engine should have consumed some mass." );
            Assert.That( engine.MassConsumedLastStep, Is.LessThan( tank.Contents.GetMass() ), "Engine must not instantly consume whole tank; consumption limited by injector conductance." );
            Assert.That( tank.Contents.GetMass(), Is.LessThan( 800 ), "Tank should have lost some mass but not be emptied in a single step." );
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

            var sourceTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 0, 10, 0 ) );
            sourceTank.Contents.Add( TestSubstances.Water, 10000 ); // Full

            var stiffSink = FlowNetworkTestHelper.CreateTestTank( 0.1, gravity, Vector3.zero ); // Tiny 100L tank
            stiffSink.Contents.Add( TestSubstances.Water, 99.9 ); // 99.9% full, very stiff

            builder.TryAddFlowObj( new object(), sourceTank );
            builder.TryAddFlowObj( new object(), stiffSink );

            CreateAndAddPipe( builder, sourceTank, new Vector3( 0, 9, 0 ), stiffSink, new Vector3( 0, 1, 0 ), 100.0 ); // High conductance pipe

            var snapshot = builder.BuildSnapshot();

            // Act: Run a single step. The solver's damping should prevent a massive overshoot.
            snapshot.Step( 0.02f );

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
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 0, 10, 0 ) );
            double initialMass = 0.1; // Very little fuel
            tank.Contents.Add( TestSubstances.Kerosene, initialMass );

            var engine = new EngineFeedSystem( 0.01 )
            {
                IsOutflowEnabled = true,
                PumpPressureRise = 50e5, // Strong suction
                ChamberPressure = 1e5,
                InjectorConductance = 1.0, // High demand
                Demand = 1000.0
            };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), engine );
            CreateAndAddPipe( builder, tank, new Vector3( 0, 9, 0 ), engine, Vector3.zero, 1.0 );

            var snapshot = builder.BuildSnapshot();

            // Act: Run for a few steps, enough to drain the tank.
            for( int i = 0; i < 50; i++ )
            {
                snapshot.Step( 0.02f );
            }

            // Assert
            Assert.That( tank.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 1e-9 ), "Tank should be completely empty." );

            double totalMassConsumed = 0;
            // The engine's consumption is stateful, so we need to simulate its own ApplyFlows logic
            // to find the total consumed mass over the simulation period.
            var tempEngine = new EngineFeedSystem( 0.01 )
            {
                IsOutflowEnabled = true,
                PumpPressureRise = 50e5,
                ChamberPressure = 1e5,
                InjectorConductance = 1.0,
                Demand = 1000.0
            };
            var tempTank = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero );
            tempTank.Contents.Add( TestSubstances.Kerosene, initialMass );

            var tempBuilder = new FlowNetworkBuilder();
            tempBuilder.TryAddFlowObj( new object(), tempTank );
            tempBuilder.TryAddFlowObj( new object(), tempEngine );
            CreateAndAddPipe( tempBuilder, tempTank, new Vector3( 0, -1, 0 ), tempEngine, Vector3.zero, 1.0 );
            var tempSnapshot = tempBuilder.BuildSnapshot();
            for( int i = 0; i < 5; i++ )
            {
                tempSnapshot.Step( 0.02f );
                tempEngine.ApplyFlows( 0.02f );
                totalMassConsumed += tempEngine.MassConsumedLastStep;
            }

            Assert.That( totalMassConsumed, Is.EqualTo( initialMass ).Within( 1e-9 ), "Engine should have consumed exactly the mass available in the tank." );
            Assert.That( snapshot.Pipes[0].ComputeFlowRate( tank.Sample( Vector3.zero, 0.1 ).FluidSurfacePotential, engine.Sample( Vector3.zero, 0.1 ).FluidSurfacePotential ), Is.Not.NaN, "Flow rate should not be NaN after draining." );
        }
    }
}