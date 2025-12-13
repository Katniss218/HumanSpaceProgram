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

        [Test]
        public void LiquidFlow___TwoIdenticalHorizontalTanks___LevelsEqualize()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -2, 0, 0 ), TestSubstances.Water, 1000 ); // Full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 2, 0, 0 ) ); // Empty

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -2, -1, 0 ), tankB, new Vector3( 2, -1, 0 ), 1.0f );

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank A should have half the mass." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 500.0 ).Within( 5.0 ), "Tank B should have half the mass." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
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
            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 400; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 6.0 ).Within( 0.01 ) );
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 2.0 ).Within( 0.2 ), "Small tank should settle at 1/3 total mass." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 4.0 ).Within( 0.2 ), "Large tank should settle at 2/3 total mass." );
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

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank should be empty." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.1 ), "Destination tank should be full." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
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
            pipe.HeadAdded = 50; // Overcome 5m height difference (potential diff = g*h = 10*10 = 100)

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( tankA.Contents.GetMass(), Is.LessThan( 600.0 ), "Source tank should be roughly half full." );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 400.0 ), "Destination tank should be roughly half full." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
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

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank should be empty." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.1 ), "Destination tank should be full." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
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

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 200; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( sink.Contents.GetMass(), Is.GreaterThan( 0 ) );
            Assert.That( sourceA.Contents.GetMass(), Is.LessThan( 500 ) );
            Assert.That( sourceB.Contents.GetMass(), Is.LessThan( 500 ) );
            Assert.That( sourceA.Contents.GetMass() + sourceB.Contents.GetMass() + sink.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
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

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 1000; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank A should be empty." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Intermediate tank B should be empty (or have minimal residual fluid)." );
            Assert.That( tankC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.1 ), "Final tank C should be full." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
        }

        [Test]
        public void LiquidFlow_CircularNetwork_NoPumps_IsStable()
        {
            // Arrange
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

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
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

            // Symmetrical pipes
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( -1, 9, 0 ), sinkB, new Vector3( -4, 1, 0 ), 1.0f );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 1, 9, 0 ), sinkC, new Vector3( 4, 1, 0 ), 1.0f );

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( source.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 0.1 ), "Source tank should be empty." );
            Assert.That( sinkB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 5.0 ), "Sink B should be full." );
            Assert.That( sinkC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 5.0 ), "Sink C should be full." );
            Assert.That( source.Contents.GetMass() + sinkB.Contents.GetMass() + sinkC.Contents.GetMass(), Is.EqualTo( 2000.0 ).Within( 1e-6 ) );
        }

        [Test]
        public void Pump_CreatesStableHeightDifference_AndHoldsEquilibrium()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( -2, 0, 0 ), TestSubstances.Water, 1500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( 2, 5, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -1, -1, 0 ), tankB, new Vector3( 1, -1, 0 ), 1.0f );
            pipe.HeadAdded = 50; // g*h -> 10 * h = 50 -> h_diff should be ~5m

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 200; i++ ) // Simulate for a long time to ensure stability
            {
                //int pipeIndex2 = snapshot.Pipes.ToList().IndexOf( pipe );
                //Debug.Log( snapshot.CurrentFlowRates[pipeIndex2] + " : " + tankA.Contents.GetMass() + " -> " + tankB.Contents.GetMass() );
                snapshot.Step( (float)DT );
            }

            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.5 ).Percent, "Tank A should have equalized mass." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.5 ).Percent, "Tank B should have equalized mass." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 2000.0 ).Within( 1e-6 ), "Mass must be conserved." );

            // Check for equilibrium
            int pipeIndex = snapshot.Pipes.ToList().IndexOf( pipe );
            Assert.That( snapshot.CurrentFlowRates[pipeIndex], Is.EqualTo( 0.0 ).Within( 1e-1 ), "Flow rate should be near zero at equilibrium." );
        }

        [Test]
        public void LiquidFlow_CircularNetwork_WithPump_CirculatesAndConservesMass()
        {
            // Arrange
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

            pipeAB.HeadAdded = 100; // Add a pump to one pipe

            var snapshot = builder.BuildSnapshot();

            // Act
            // Run for a short time to see initial flow
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert initial circulation
            int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
            Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.GreaterThan( 0.1 ), "Pump should induce positive flow in pipe A->B." );

            // Run for longer to see if it's stable
            for( int i = 0; i < 490; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert final state
            double totalMass = tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass();
            Assert.That( totalMass, Is.EqualTo( 1500.0 ).Within( 1e-6 ), "Total mass must be conserved." );

            // Flow should be happening, but may not be perfectly stable. Just check it's not zero.
            Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.GreaterThan( 0.1 ), "Flow should be maintained by the pump." );
        }

        [Test]
        public void AdvancedPump_MultipleModifiersOnSinglePipe_HeadsAreAdditive()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ) ); // 10m higher

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, Vector3.zero, tankB, new Vector3( 0, 10, 0 ), 1.0f );

            var pump1 = new PumpModifier { HeadAdded = 60 };
            var pump2 = new PumpModifier { HeadAdded = 60 };

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 500; i++ )
            {
                pipe.HeadAdded = 0; // Reset head each step
                pump1.Apply( pipe );  // Manually apply modifiers to simulate what FResourceConnection_FlowPipe would do.
                pump2.Apply( pipe );
                snapshot.Step( (float)DT );
            }

            // Assert
            Assert.That( pipe.HeadAdded, Is.EqualTo( 120.0 ) );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 10.0 ), "Fluid should flow uphill with combined pump power." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
        }

        [Test]
        public void AdvancedPump___SeriesPumps___PressureBuildsInIntermediateTank()
        {
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -10, 0, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 0.1, GRAVITY, Vector3.zero ); // Small intermediate tank
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 10, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -9, 0, 0 ), tankB, new Vector3( -1, 0, 0 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, 0, 0 ), tankC, new Vector3( 9, 0, 0 ), 1.0f );

            var pumpAB = new PumpModifier { HeadAdded = 100 }; // Pump A->B
            var pumpBC = new PumpModifier { HeadAdded = -50 }; // Pump C->B (resists flow)
            pumpAB.Apply( pipeAB );
            pumpBC.Apply( pipeBC );

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 100; i++ )
            {
                snapshot.Step( (float)DT );
                tankA.RunInternalSimulationStep( DT );
                tankB.RunInternalSimulationStep( DT );
                tankC.RunInternalSimulationStep( DT );
            }

            Assert.That( tankB.FluidState.Pressure, Is.GreaterThan( FluidState.STP.Pressure * 1.5 ), "Pressure in intermediate tank should be elevated." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
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
            var snapshot1 = builder1.BuildSnapshot();

            for( int i = 0; i < 100; i++ )
            {
                snapshot1.Step( (float)DT );
            }
            double massTransferredSingle = tankB1.Contents.GetMass();

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
            var snapshot2 = builder2.BuildSnapshot();

            for( int i = 0; i < 100; i++ )
            {
                snapshot2.Step( (float)DT );
            }
            double massTransferredDouble = tankB2.Contents.GetMass();

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

            // Create a 2x2 grid of pipes with pumps, connecting the centers of the tanks.
            var pipeA1B1 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA1, new Vector3( -5, 5, 0 ), sinkB1, new Vector3( -5, -5, 0 ), 1.0f );
            var pipeA1B2 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA1, new Vector3( -5, 5, 0 ), sinkB2, new Vector3( 5, -5, 0 ), 1.0f );
            var pipeA2B1 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA2, new Vector3( 5, 5, 0 ), sinkB1, new Vector3( -5, -5, 0 ), 1.0f );
            var pipeA2B2 = FlowNetworkTestHelper.CreateAndAddPipe( builder, sourceA2, new Vector3( 5, 5, 0 ), sinkB2, new Vector3( 5, -5, 0 ), 1.0f );

            pipeA1B1.HeadAdded = 100;
            pipeA1B2.HeadAdded = 50;
            pipeA2B1.HeadAdded = 50;
            pipeA2B2.HeadAdded = 100;

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
            }

            double totalMass = sourceA1.Contents.GetMass() + sourceA2.Contents.GetMass() + sinkB1.Contents.GetMass() + sinkB2.Contents.GetMass();
            Assert.That( totalMass, Is.EqualTo( 1000.0 ).Within( 1e-6 ), "Mass must be conserved." );
            Assert.That( sinkB1.Contents.GetMass(), Is.GreaterThan( 0 ) );
            Assert.That( sinkB2.Contents.GetMass(), Is.GreaterThan( 0 ) );
            // Since the pumps are symmetrical (A1->B1 and A2->B2 are strong, A1->B2 and A2->B1 are weak),
            // we expect B1 and B2 to fill roughly equally.
            Assert.That( sinkB1.Contents.GetMass(), Is.EqualTo( sinkB2.Contents.GetMass() ).Within( 10 ).Percent );
        }

        [Test]
        public void MultiSubstance___MixedLiquidFlows___CompositionIsPreserved()
        {
            var builder = new FlowNetworkBuilder();
            var source = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, Vector3.zero );
            source.Contents.Add( TestSubstances.Water, 500 );
            source.Contents.Add( TestSubstances.Oil, 400 ); // 500kg water, 400kg oil. Total 900kg.
            var sink = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, new Vector3( 0, -5, 0 ) );

            builder.TryAddFlowObj( new object(), source );
            builder.TryAddFlowObj( new object(), sink );
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 0, -1, 0 ), sink, new Vector3( 0, -4, 0 ), 1.0f );
            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 100; i++ )
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( sink.Contents.Count, Is.EqualTo( 2 ) );
            Assert.That( sink.Contents.Contains( TestSubstances.Water ), Is.True );
            Assert.That( sink.Contents.Contains( TestSubstances.Oil ), Is.True );

            double ratioInSink = sink.Contents[TestSubstances.Water] / sink.Contents[TestSubstances.Oil];
            Assert.That( ratioInSink, Is.EqualTo( 500.0 / 400.0 ).Within( 0.1 ).Percent, "Mixture ratio in sink should match source." );
        }

        [Test]
        public void MultiPhaseFlow___StratifiedLiquidTank___DrainsCorrectly()
        {
            var builder = new FlowNetworkBuilder();
            var source = FlowNetworkTestHelper.CreateTestTank( 2.0, GRAVITY, Vector3.zero );
            source.Contents.Add( TestSubstances.Water, 1000 );   // Density 1000, will be on top
            source.Contents.Add( TestSubstances.Mercury, 13500 ); // Density 13500, will be at bottom

            // Sinks must be below the source for gravity flow.
            var waterSink = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, -5, 0 ) );
            var mercurySink = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, -5, 0 ) );

            builder.TryAddFlowObj( new object(), source );
            builder.TryAddFlowObj( new object(), waterSink );
            builder.TryAddFlowObj( new object(), mercurySink );

            // Drain from top half of source (water layer) to the top of the waterSink.
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 0, 0.5f, 0 ), waterSink, new Vector3( 5, -4, 0 ), 1.0f );
            // Drain from bottom half of source (mercury layer) to the top of the mercurySink.
            FlowNetworkTestHelper.CreateAndAddPipe( builder, source, new Vector3( 0, -0.5f, 0 ), mercurySink, new Vector3( -5, -4, 0 ), 1.0f );

            var snapshot = builder.BuildSnapshot();

            for( int i = 0; i < 5; i++ ) // Drain only for a few steps, so that water level doesn't drop all the way to the mercury inlet.
            {
                snapshot.Step( (float)DT );
            }

            Assert.That( waterSink.Contents.GetMass(), Is.GreaterThan( 0 ), "Water sink should have received mass." );
            Assert.That( waterSink.Contents.Contains( TestSubstances.Water ), Is.True, "Water sink should contain water." );
            Assert.That( waterSink.Contents.Contains( TestSubstances.Mercury ), Is.False, "Water sink should not contain mercury." );

            Assert.That( mercurySink.Contents.GetMass(), Is.GreaterThan( 0 ), "Mercury sink should have received mass." );
            Assert.That( mercurySink.Contents.Contains( TestSubstances.Mercury ), Is.True, "Mercury sink should contain mercury." );
            Assert.That( mercurySink.Contents.Contains( TestSubstances.Water ), Is.False, "Mercury sink should not contain water." );
        }

        [Test]
        public void UnusualTopology_PipeConnectingSameTank_IsStableWithZeroFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero, TestSubstances.Water, 1000 );

            var topInletPos = new Vector3( 0, 1, 0 );
            var bottomInletPos = new Vector3( 0, -1, 0 );

            builder.TryAddFlowObj( new object(), tank );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tank, topInletPos, tank, bottomInletPos, 1.0f );

            var snapshot = builder.BuildSnapshot();
            double initialMass = tank.Contents.GetMass();

            // Act
            for( int i = 0; i < 200; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-6 ), "Mass should be conserved within the tank." );

            int pipeIdx = snapshot.Pipes.ToList().IndexOf( pipe );
            Assert.That( snapshot.CurrentFlowRates[pipeIdx], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow rate in a self-connecting pipe should be zero." );
        }

        [Test]
        public void UnusualTopology_PipeToDisabledConsumer_HasZeroFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero, TestSubstances.Water, 1000 );
            var consumer = new GenericConsumer { IsEnabled = false, Demand = 10.0 };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), consumer );

            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), consumer, new Vector3( 0, -5, 0 ), 1.0f );

            var snapshot = builder.BuildSnapshot();
            double initialMass = tank.Contents.GetMass();

            // Act
            for( int i = 0; i < 200; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-6 ), "Mass should not leave the tank." );

            int pipeIdx = snapshot.Pipes.ToList().IndexOf( pipe );
            Assert.That( snapshot.CurrentFlowRates[pipeIdx], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow rate to a disabled consumer should be zero." );
            Assert.That( consumer.Inflow.IsEmpty(), Is.True, "Disabled consumer should not receive any inflow." );
        }

        [Test]
        public void UnusualTopology_PipeToZeroDemandConsumer_TransfersNoMass()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tank = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 1, 0 ), TestSubstances.Water, 1000 ); // Tank is above
            var consumer = new GenericConsumer { IsEnabled = true, Demand = 0.0 };

            builder.TryAddFlowObj( new object(), tank );
            builder.TryAddFlowObj( new object(), consumer );

            // Pipe goes downhill, so there is a positive potential gradient
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tank, new Vector3( 0, -1, 0 ), consumer, new Vector3( 0, -5, 0 ), 1.0f );

            var snapshot = builder.BuildSnapshot();
            double initialMass = tank.Contents.GetMass();

            // Act
            for( int i = 0; i < 200; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            Assert.That( tank.Contents.GetMass(), Is.EqualTo( initialMass ).Within( 1e-6 ), "Mass should not leave the tank with zero demand." );
            Assert.That( consumer.Inflow.IsEmpty(), Is.True, "Zero-demand consumer should not receive any inflow." );
            // Note: The solver may calculate a non-zero ideal flow rate due to the GenericConsumer's passive suction,
            // but the ApplyTransport step correctly throttles the actual mass transfer to zero because GetAvailableInflowVolume is zero.
            // The most important assertion is that mass is conserved.
        }

        [Test]
        public void UnusualTopology_OpposingPumps_EqualStrength_ResultsInZeroFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ), TestSubstances.Water, 500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 5 ), TestSubstances.Water, 500 );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            // Clockwise loop A->B->C->A
            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -4, -1, 0 ), tankB, new Vector3( -1, -1, 5 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, -1, 5 ), tankC, new Vector3( 4, -1, 0 ), 1.0f );
            var pipeCA = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankC, new Vector3( 5, -1, -1 ), tankA, new Vector3( -5, -1, -1 ), 1.0f );

            // Pump A->B is clockwise. Pump B->C is counter-clockwise (pushes from C to B, so negative head).
            pipeAB.HeadAdded = 50.0;
            pipeBC.HeadAdded = -50.0;

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 200; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 1 ).Percent );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1 ).Percent );
            Assert.That( tankC.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 1 ).Percent );

            int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
            int pipeBC_idx = snapshot.Pipes.ToList().IndexOf( pipeBC );
            int pipeCA_idx = snapshot.Pipes.ToList().IndexOf( pipeCA );

            Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
            Assert.That( snapshot.CurrentFlowRates[pipeBC_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
            Assert.That( snapshot.CurrentFlowRates[pipeCA_idx], Is.EqualTo( 0.0 ).Within( 1e-6 ) );
        }

        [Test]
        public void UnusualTopology_OpposingPumps_UnequalStrength_ResultsInNetFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( -5, 0, 0 ), TestSubstances.Water, 500 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 0, 5 ), TestSubstances.Water, 500 );
            var tankC = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 5, 0, 0 ), TestSubstances.Water, 500 );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            builder.TryAddFlowObj( new object(), tankC );

            // Clockwise loop A->B->C->A
            var pipeAB = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( -4, -1, 0 ), tankB, new Vector3( -1, -1, 5 ), 1.0f );
            var pipeBC = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankB, new Vector3( 1, -1, 5 ), tankC, new Vector3( 4, -1, 0 ), 1.0f );
            var pipeCA = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankC, new Vector3( 5, -1, -1 ), tankA, new Vector3( -5, -1, -1 ), 1.0f );

            // Stronger clockwise pump vs weaker counter-clockwise pump. Net flow should be clockwise.
            pipeAB.HeadAdded = 100.0;
            pipeBC.HeadAdded = -50.0;

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 50; i++ ) // Run for a shorter time to see flow, not equilibrium
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            double totalMass = tankA.Contents.GetMass() + tankB.Contents.GetMass() + tankC.Contents.GetMass();
            Assert.That( totalMass, Is.EqualTo( 1500.0 ).Within( 1e-6 ), "Total mass must be conserved." );

            int pipeAB_idx = snapshot.Pipes.ToList().IndexOf( pipeAB );
            int pipeBC_idx = snapshot.Pipes.ToList().IndexOf( pipeBC );
            int pipeCA_idx = snapshot.Pipes.ToList().IndexOf( pipeCA );

            // Net flow should be clockwise (A->B->C->A), so all flow rates should be positive.
            Assert.That( snapshot.CurrentFlowRates[pipeAB_idx], Is.GreaterThan( 0.1 ), "Net flow should be established in the dominant (clockwise) direction." );
            Assert.That( snapshot.CurrentFlowRates[pipeBC_idx], Is.GreaterThan( 0.1 ) );
            Assert.That( snapshot.CurrentFlowRates[pipeCA_idx], Is.GreaterThan( 0.1 ) );
        }
    }
}