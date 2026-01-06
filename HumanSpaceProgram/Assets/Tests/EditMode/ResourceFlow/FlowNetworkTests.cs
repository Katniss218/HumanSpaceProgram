using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkTests
    {
        private const double DT = 0.02;
        private static readonly Vector3 GRAVITY = new Vector3( 0, -10, 0 );

        [Test, Description( "Validates that the new job-based solver correctly equalizes fluid levels between two tanks." )]
        public void LiquidFlow_TwoIdenticalHorizontalTanks_LevelsEqualize()
        {
            // ARRANGE: Build the network inside the test method.
            var builder = new FlowNetworkBuilder();

            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -2, 0, 0 ), TestSubstances.Water, 1000 ); // Full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 2, 0, 0 ) ); // Empty

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -2, -1, 0 ), tankB, new Vector3( 2, -1, 0 ), 1.0f );

            // The 'using' block ensures snapshot.Dispose() is called automatically,
            // even if the test fails with an exception.
            using( var snapshot = builder.BuildSnapshot() )
            {
                // ACT: The simulation loop now consists of two phases, just like the FlowNetworkManager.
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                // ASSERT: The assertions are the same as before. We check the final state
                // of the C# objects after the simulation has run.
                double massA = tankA.Contents.GetMass();
                double massB = tankB.Contents.GetMass();

                Assert.That( massA, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank A should have half the mass." );
                Assert.That( massB, Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank B should have half the mass." );
                Assert.That( massA + massB, Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
            }
        }

        [Test, Description( "A simple test to ensure a network with no pipes doesn't cause errors and conserves mass." )]
        public void NoPipes_SolverHandlesEmptyNetwork_Gracefully()
        {
            // ARRANGE: A different network setup with only one tank and no pipes.
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -2, 0, 0 ), TestSubstances.Water, 1000 );
            builder.TryAddFlowObj( new object(), tankA );

            using( var snapshot = builder.BuildSnapshot() )
            {
                // ACT & ASSERT: The main assertion is that this doesn't throw an exception.
                Assert.DoesNotThrow( () =>
                {
                    for( int i = 0; i < 10; i++ )
                    {
                        snapshot.PrepareAndSolve( (float)DT );
                        snapshot.ApplyResults( (float)DT );
                    }
                } );

                // Also assert that mass hasn't changed.
                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-9 ) );
            }
        }

        [Test]
        public void GasFlow___HighPressureToLowPressure_Horizontal___PressuresEqualizeByVolume()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( -2, 0, 0 ), TestSubstances.Air, 6.0 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 2.0, Vector3.zero, new Vector3( 2, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -1, 0, 0 ), tankB, new Vector3( 1, 0, 0 ), 0.1f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 400; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 6.0 ).Within( 0.01 ) );
                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 2.0 ).Within( 0.2 ), "Small tank should settle at 1/3 total mass." );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 4.0 ).Within( 0.2 ), "Large tank should settle at 2/3 total mass." );
            }
        }

        [Test]
        public void LiquidFlow___FullTankToEmptyLowerTank___TransfersCompletely()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ), TestSubstances.Water, 1000 ); // High tank, full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero ); // Low tank, empty

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 9, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank should be empty." );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.1 ), "Destination tank should be full." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
            }
        }

        [Test]
        public void LiquidFlow_PumpUphill_TransfersFluid()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero, TestSubstances.Water, 1000 ); // Low tank, full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 5, 0 ) ); // High tank, empty

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, 4, 0 ), 1.0f );
            pipe.HeadAdded = 50; // Overcome 5m height difference

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.LessThan( 600.0 ), "Source tank should be roughly half full." );
                Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 400.0 ), "Destination tank should be roughly half full." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
            }
        }

        [Test]
        public void LiquidFlow_StrongPumpUphill_TransfersFluid()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero, TestSubstances.Water, 1000 ); // Low tank, full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 5, 0 ) ); // High tank, empty

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, 4, 0 ), 1.0f );
            pipe.HeadAdded = 200;

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank should be empty." );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.1 ), "Destination tank should be full." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
            }
        }

        [Test]
        public void LiquidFlow___TwoSourcesToOneSink___FlowsMergeCorrectly()
        {
            var builder = new FlowNetworkBuilder();
            var sourceA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 10, 0 ), TestSubstances.Water, 500 );
            var sourceB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 10, 0 ), TestSubstances.Water, 500 );
            var sink = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, Vector3.zero );

            builder.TryAddFlowObj( new object(), sourceA );
            builder.TryAddFlowObj( new object(), sourceB );
            builder.TryAddFlowObj( new object(), sink );

            FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA, new Vector3( -5, 9, 0 ), sink, new Vector3( -1, 1, 0 ), 1.0f );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceB, new Vector3( 5, 9, 0 ), sink, new Vector3( 1, 1, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 200; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( sink.Contents.GetMass(), Is.GreaterThan( 0 ) );
                Assert.That( sourceA.Contents.GetMass(), Is.LessThan( 500 ) );
                Assert.That( sourceB.Contents.GetMass(), Is.LessThan( 500 ) );
                Assert.That( sourceA.Contents.GetMass() + sourceB.Contents.GetMass() + sink.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void LiquidFlow_SeriesTanks_FlowsThroughCorrectly()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 20, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ) );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 19, 0 ), tankB, new Vector3( 0, 11, 0 ), 1.0f );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 0, 9, 0 ), tankC, new Vector3( 0, 1, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 1000; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank A should be empty." );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Intermediate tank B should be empty (or have minimal residual fluid)." );
                Assert.That( tankC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.1 ), "Final tank C should be full." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void LiquidFlow_CircularNetwork_NoPumps_IsStable()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ), TestSubstances.Water, 500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 5 ), TestSubstances.Water, 500 );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -4, 0, 0 ), tankB, new Vector3( -1, 0, 5 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, 0, 5 ), tankC, new Vector3( 4, 0, 0 ), 1.0f );
            var pipeCA = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankC, new Vector3( 5, 0, -1 ), tankA, new Vector3( -5, 0, -1 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 500.0 ).Within( 1e-6 ), "Tank A mass should not change." );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 500.0 ).Within( 1e-6 ), "Tank B mass should not change." );
                Assert.That( tankC.Contents.GetMass(), Is.EqualTo( 500.0 ).Within( 1e-6 ), "Tank C mass should not change." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass(), Is.EqualTo( 1500.0 ).Within( 1e-6 ) );

                int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
                int pipeBC_idx = snapshot.Pipes.ToList().IndexOf( pipeBC );
                int pipeCA_idx = snapshot.Pipes.ToList().IndexOf( pipeCA );

                Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow in pipe A->B should be zero." );
                Assert.That( snapshot.CurrentFlowRates[pipeBC_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow in pipe B->C should be zero." );
                Assert.That( snapshot.CurrentFlowRates[pipeCA_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow in pipe C->A should be zero." );
            }
        }

        [Test]
        public void LiquidFlow_ParallelSinks_FlowSplitsCorrectly()
        {
            var builder = new FlowNetworkBuilder();
            var source = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( 0, 10, 0 ), TestSubstances.Water, 2000 );
            var sinkB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ) );
            var sinkC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ) );

            builder.TryAddFlowObj( new object(), source );
            builder.TryAddFlowObj( new object(), sinkB );
            builder.TryAddFlowObj( new object(), sinkC );

            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( -1, 9, 0 ), sinkB, new Vector3( -4, 1, 0 ), 1.0f );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 1, 9, 0 ), sinkC, new Vector3( 4, 1, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( source.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank should be empty." );
                Assert.That( sinkB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 5.0 ), "Sink B should be full." );
                Assert.That( sinkC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 5.0 ), "Sink C should be full." );
                Assert.That( source.Contents.GetMass() + sinkB.Contents.GetMass() + sinkC.Contents.GetMass(), Is.EqualTo( 2000.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void Pump_CreatesStableHeightDifference_AndHoldsEquilibrium()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( 0, 0, 0 ), TestSubstances.Water, 1500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( 0, 5, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, 4, 0 ), 1.0f );
            pipe.HeadAdded = 50;

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 200; i++ )
                {
                    Debug.Log( tankA.Contents.GetMass() + " | " + tankB.Contents.GetMass() );
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.5 ).Percent, "Tank A should have equalized mass." );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.5 ).Percent, "Tank B should have equalized mass." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 2000.0 ).Within( 1e-6 ), "Mass must be conserved." );

                int pipeIndex = snapshot.Pipes.ToList().IndexOf( pipe );
                Assert.That( snapshot.CurrentFlowRates[pipeIndex], Is.EqualTo( 0.0 ).Within( 1e-1 ), "Flow rate should be near zero at equilibrium." );
            }
        }

        [Test]
        public void LiquidFlow_CircularNetwork_WithPump_CirculatesAndConservesMass()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ), TestSubstances.Water, 500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 5 ), TestSubstances.Water, 500 );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -4, 0, 0 ), tankB, new Vector3( -1, 0, 5 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, 0, 5 ), tankC, new Vector3( 4, 0, 0 ), 1.0f );
            var pipeCA = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankC, new Vector3( 5, 0, -1 ), tankA, new Vector3( -5, 0, -1 ), 1.0f );

            pipeAB.HeadAdded = 100;

            using( var snapshot = builder.BuildSnapshot() )
            {
                // Initial flow check
                for( int i = 0; i < 10; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }
                int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
                Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.GreaterThan( 0.1 ), "Pump should induce positive flow in pipe A->B." );

                // Stability check
                for( int i = 0; i < 490; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                double totalMass = tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass();
                Assert.That( totalMass, Is.EqualTo( 1500.0 ).Within( 0.1 ).Percent, "Total mass must be conserved." );
                Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.GreaterThan( 0.1 ), "Flow should be maintained by the pump." );
            }
        }

        [Test]
        public void AdvancedPump_MultipleModifiersOnSinglePipe_HeadsAreAdditive()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, Vector3.zero, tankB, new Vector3( 0, 10, 0 ), 1.0f );

            var pump1 = new PumpModifier { HeadAdded = 60 };
            var pump2 = new PumpModifier { HeadAdded = 60 };

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    // Manually simulate modifiers as we don't have components
                    pipe.HeadAdded = 0;
                    pump1.Apply( pipe );
                    pump2.Apply( pipe );

                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( pipe.HeadAdded, Is.EqualTo( 120.0 ) );
                Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 10.0 ), "Fluid should flow uphill with combined pump power." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void AdvancedPump___SeriesPumps___PressureBuildsInIntermediateTank()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -10, 0, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 0.1, GRAVITY, Vector3.zero );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 10, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -9, 0, 0 ), tankB, new Vector3( -1, 0, 0 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, 0, 0 ), tankC, new Vector3( 9, 0, 0 ), 1.0f );

            var pumpAB = new PumpModifier { HeadAdded = 100 };
            var pumpBC = new PumpModifier { HeadAdded = -50 };

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 100; i++ )
                {
                    // Apply manual modifiers
                    pipeAB.HeadAdded = 0; pumpAB.Apply( pipeAB );
                    pipeBC.HeadAdded = 0; pumpBC.Apply( pipeBC );

                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankB.FluidState.Pressure, Is.GreaterThan( FluidState.STP.Pressure * 1.5 ), "Pressure in intermediate tank should be elevated." );
                Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void AdvancedPump___ParallelPumps___FlowRatesSumCorrectly()
        {
            // Scenario 1: Single Pipe
            var builder1 = new FlowNetworkBuilder();
            var tankA1 = FlowNetworkTestHelper.CreateTestTank( 10.0, GRAVITY, Vector3.zero, TestSubstances.Water, 10000 );
            var tankB1 = FlowNetworkTestHelper.CreateTestTank( 10.0, GRAVITY, new Vector3( 10, 0, 0 ) );
            builder1.TryAddFlowObj( new object(), tankA1 );
            builder1.TryAddFlowObj( new object(), tankB1 );
            var pipe1 = FlowNetworkTestHelper.CreateAndAddPipe( builder1, tankA1, Vector3.right, tankB1, new Vector3( 9, 0, 0 ), 1.0f );
            pipe1.HeadAdded = 100;

            double massTransferredSingle = 0;
            using( var snapshot1 = builder1.BuildSnapshot() )
            {
                for( int i = 0; i < 100; i++ )
                {
                    snapshot1.PrepareAndSolve( (float)DT );
                    snapshot1.ApplyResults( (float)DT );
                }
                massTransferredSingle = tankB1.Contents.GetMass();
            }

            // Scenario 2: Double Pipe
            var builder2 = new FlowNetworkBuilder();
            var tankA2 = FlowNetworkTestHelper.CreateTestTank( 10.0, GRAVITY, Vector3.zero, TestSubstances.Water, 10000 );
            var tankB2 = FlowNetworkTestHelper.CreateTestTank( 10.0, GRAVITY, new Vector3( 10, 0, 0 ) );
            builder2.TryAddFlowObj( new object(), tankA2 );
            builder2.TryAddFlowObj( new object(), tankB2 );
            var pipe2a = FlowNetworkTestHelper.CreateAndAddPipe( builder2, tankA2, Vector3.right + Vector3.up, tankB2, new Vector3( 9, 1, 0 ), 1.0f );
            var pipe2b = FlowNetworkTestHelper.CreateAndAddPipe( builder2, tankA2, Vector3.right - Vector3.up, tankB2, new Vector3( 9, -1, 0 ), 1.0f );
            pipe2a.HeadAdded = 100;
            pipe2b.HeadAdded = 100;

            double massTransferredDouble = 0;
            using( var snapshot2 = builder2.BuildSnapshot() )
            {
                for( int i = 0; i < 100; i++ )
                {
                    snapshot2.PrepareAndSolve( (float)DT );
                    snapshot2.ApplyResults( (float)DT );
                }
                massTransferredDouble = tankB2.Contents.GetMass();
            }

            Assert.That( massTransferredDouble, Is.EqualTo( massTransferredSingle * 2.0 ).Within( 3.0 ).Percent, "Two parallel pipes should transfer roughly double the mass of one." );
        }

        [Test]
        public void LiquidFlow_GridWithPumps_BalancesFlow()
        {
            var builder = new FlowNetworkBuilder();
            var sourceA1 = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 5, 0 ), TestSubstances.Water, 500 );
            var sourceA2 = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 5, 0 ), TestSubstances.Water, 500 );
            var sinkB1 = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, -5, 0 ) );
            var sinkB2 = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, -5, 0 ) );

            builder.TryAddFlowObj( new object(), sourceA1 );
            builder.TryAddFlowObj( new object(), sourceA2 );
            builder.TryAddFlowObj( new object(), sinkB1 );
            builder.TryAddFlowObj( new object(), sinkB2 );

            var pipeA1B1 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA1, new Vector3( -5, 5, 0 ), sinkB1, new Vector3( -5, -5, 0 ), 1.0f );
            var pipeA1B2 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA1, new Vector3( -5, 5, 0 ), sinkB2, new Vector3( 5, -5, 0 ), 1.0f );
            var pipeA2B1 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA2, new Vector3( 5, 5, 0 ), sinkB1, new Vector3( -5, -5, 0 ), 1.0f );
            var pipeA2B2 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA2, new Vector3( 5, 5, 0 ), sinkB2, new Vector3( 5, -5, 0 ), 1.0f );

            pipeA1B1.HeadAdded = 100;
            pipeA1B2.HeadAdded = 50;
            pipeA2B1.HeadAdded = 50;
            pipeA2B2.HeadAdded = 100;

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 500; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                double totalMass = sourceA1.Contents.GetMass() + sourceA2.Contents.GetMass() + sinkB1.Contents.GetMass() + sinkB2.Contents.GetMass();
                Assert.That( totalMass, Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
                Assert.That( sinkB1.Contents.GetMass(), Is.GreaterThan( 0 ) );
                Assert.That( sinkB2.Contents.GetMass(), Is.GreaterThan( 0 ) );
                Assert.That( sinkB1.Contents.GetMass(), Is.EqualTo( sinkB2.Contents.GetMass() ).Within( 10 ).Percent );
            }
        }

        [Test]
        public void MultiSubstance___MixedLiquidFlows___CompositionIsPreserved()
        {
            var builder = new FlowNetworkBuilder();
            var source = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, Vector3.zero );
            source.Contents.Add( TestSubstances.Water, 500 );
            source.Contents.Add( TestSubstances.Oil, 400 );
            var sink = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( 0, -5, 0 ) );

            builder.TryAddFlowObj( new object(), source );
            builder.TryAddFlowObj( new object(), sink );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 0, -1, 0 ), sink, new Vector3( 0, -4, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 100; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( sink.Contents.Count, Is.EqualTo( 2 ) );
                Assert.That( sink.Contents.Contains( TestSubstances.Water ), Is.True );
                Assert.That( sink.Contents.Contains( TestSubstances.Oil ), Is.True );

                double ratioInSink = sink.Contents[TestSubstances.Water] / sink.Contents[TestSubstances.Oil];
                Assert.That( ratioInSink, Is.EqualTo( 500.0 / 400.0 ).Within( 0.1 ).Percent, "Mixture ratio in sink should match source." );
            }
        }

        [Test]
        public void MultiPhaseFlow___StratifiedLiquidTank___DrainsCorrectly()
        {
            var builder = new FlowNetworkBuilder();
            var source = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, Vector3.zero );
            source.Contents.Add( TestSubstances.Water, 1000 );
            source.Contents.Add( TestSubstances.Mercury, 13500 );

            var waterSink = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, -5, 0 ) );
            var mercurySink = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, -5, 0 ) );

            builder.TryAddFlowObj( new object(), source );
            builder.TryAddFlowObj( new object(), waterSink );
            builder.TryAddFlowObj( new object(), mercurySink );

            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 0, 0.5f, 0 ), waterSink, new Vector3( 5, -4, 0 ), 1.0f );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 0, -0.5f, 0 ), mercurySink, new Vector3( -5, -4, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 5; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( waterSink.Contents.GetMass(), Is.GreaterThan( 0 ) );
                Assert.That( waterSink.Contents.Contains( TestSubstances.Water ), Is.True );
                Assert.That( waterSink.Contents.Contains( TestSubstances.Mercury ), Is.False );

                Assert.That( mercurySink.Contents.GetMass(), Is.GreaterThan( 0 ) );
                Assert.That( mercurySink.Contents.Contains( TestSubstances.Mercury ), Is.True );
                Assert.That( mercurySink.Contents.Contains( TestSubstances.Water ), Is.False );
            }
        }

        [Test]
        public void UnusualTopology_PipeConnectingSameTank_IsStableWithZeroFlow()
        {
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero, TestSubstances.Water, 1000 );

            builder.TryAddFlowObj( new object(), tank );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tank, new Vector3( 0, 1, 0 ), tank, new Vector3( 0, -1, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                double initialMass = tank.Contents.GetMass();
                for( int i = 0; i < 200; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-6 ) );
                int pipeIdx = snapshot.Pipes.ToList().IndexOf( pipe );
                Assert.That( snapshot.CurrentFlowRates[pipeIdx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void UnusualTopology_PipeToDisabledConsumer_HasZeroFlow()
        {
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero, TestSubstances.Water, 1000 );
            var consumer = new GenericConsumer { IsEnabled = false, Demand = 10.0 };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), consumer );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), consumer, new Vector3( 0, -5, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                double initialMass = tank.Contents.GetMass();
                for( int i = 0; i < 200; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-6 ) );
                int pipeIdx = snapshot.Pipes.ToList().IndexOf( pipe );
                Assert.That( snapshot.CurrentFlowRates[pipeIdx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
                Assert.That( consumer.Inflow.IsEmpty(), Is.True );
            }
        }

        [Test]
        public void UnusualTopology_PipeToZeroDemandConsumer_TransfersNoMass()
        {
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 1, 0 ), TestSubstances.Water, 1000 );
            var consumer = new GenericConsumer { IsEnabled = true, Demand = 0.0 };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), consumer );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), consumer, new Vector3( 0, -5, 0 ), 1.0f );

            using( var snapshot = builder.BuildSnapshot() )
            {
                double initialMass = tank.Contents.GetMass();
                for( int i = 0; i < 200; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-6 ) );
                Assert.That( consumer.Inflow.IsEmpty(), Is.True );
            }
        }

        [Test]
        public void UnusualTopology_OpposingPumps_EqualStrength_ResultsInZeroFlow()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ), TestSubstances.Water, 500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 5 ), TestSubstances.Water, 500 );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -5, -1, 0 ), tankB, new Vector3( 0, -1, 5 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 0, -1, 5 ), tankC, new Vector3( 5, -1, 0 ), 1.0f );
            var pipeCA = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankC, new Vector3( 5, -1, 0 ), tankA, new Vector3( -5, -1, 0 ), 1.0f );

            pipeAB.HeadAdded = 50.0;
            pipeBC.HeadAdded = -50.0;

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 200; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 1 ).Percent );
                Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1 ).Percent );
                Assert.That( tankC.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 1 ).Percent );

                int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
                int pipeBC_idx = snapshot.Pipes.ToList().IndexOf( pipeBC );
                Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
                Assert.That( snapshot.CurrentFlowRates[pipeBC_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
            }
        }

        [Test]
        public void UnusualTopology_OpposingPumps_UnequalStrength_ResultsInNetFlow()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ), TestSubstances.Water, 500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 5 ), TestSubstances.Water, 500 );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -4, -1, 0 ), tankB, new Vector3( -1, -1, 5 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, -1, 5 ), tankC, new Vector3( 4, -1, 0 ), 1.0f );
            var pipeCA = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankC, new Vector3( 5, -1, -1 ), tankA, new Vector3( -5, -1, -1 ), 1.0f );

            pipeAB.HeadAdded = 100.0;
            pipeBC.HeadAdded = -50.0;

            using( var snapshot = builder.BuildSnapshot() )
            {
                for( int i = 0; i < 50; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );
                }

                double totalMass = tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass();
                Assert.That( totalMass, Is.EqualTo( 1500.0 ).Within( 1e-6 ) );

                int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
                Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.GreaterThan( 0.1 ), "Net flow should be clockwise." );
            }
        }

        [Test, Description( "Benchmarks the refactored solver performance on a large network." )]
        public void Performance_LargeNetwork_Benchmark()
        {
            int chainLength = 100;
            var builder = new FlowNetworkBuilder();
            var tanks = new FlowTank[chainLength];
            for( int i = 0; i < chainLength; i++ )
            {
                tanks[i] = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( i * 2, 0, 0 ) );
                builder.TryAddFlowObj( new object(), tanks[i] );
            }
            tanks[0].Contents.Add( TestSubstances.Water, 10000 ); // Fill first tank

            for( int i = 0; i < chainLength - 1; i++ )
            {
                FlowNetworkTestHelper.CreateAndAddPipe( builder, tanks[i], new Vector3( i * 2 + 1, 0, 0 ), tanks[i + 1], new Vector3( (i + 1) * 2 - 1, 0, 0 ), 1.0f );
            }

            using( var snapshot = builder.BuildSnapshot() )
            {
                // Warmup
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );
                snapshot.PrepareAndSolve( (float)DT );
                snapshot.ApplyResults( (float)DT );

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                sw.Start();
                int steps = 1000;
                for( int i = 0; i < steps; i++ )
                {
                    snapshot.PrepareAndSolve( (float)DT );
                    snapshot.ApplyResults( (float)DT );

                }
                sw.Stop();
                Debug.Log( $"Job Solver: {steps} steps with {chainLength} tanks took {sw.ElapsedMilliseconds} ms." );
            }
        }
    }
}