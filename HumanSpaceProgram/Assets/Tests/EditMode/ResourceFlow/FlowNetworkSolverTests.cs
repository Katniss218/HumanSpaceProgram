using HSP.ResourceFlow;
using NUnit.Framework;
using UnityEngine;
using HSP_Tests;
using System.Linq;
using System.Reflection;
using System;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkSolverTests
    {
        private const double DT = 0.02;

        /// <summary>
        /// Helper to access internal fields of the snapshot for white-box testing.
        /// </summary>
        private T GetInternalField<T>( FlowNetworkSnapshot snapshot, string fieldName )
        {
            var fieldInfo = typeof( FlowNetworkSnapshot ).GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance );
            if( fieldInfo == null )
            {
                Assert.Fail( $"Internal field '{fieldName}' not found on FlowNetworkSnapshot. Test needs updating due to refactor." );
                return default;
            }
            return (T)fieldInfo.GetValue( snapshot );
        }

        private static FlowPipe CreateAndAddPipe( FlowNetworkBuilder builder, IResourceConsumer from, Vector3 fromLocation, IResourceConsumer to, Vector3 toLocation, double length, float area = 0.1f )
        {
            var portA = new FlowPipe.Port( from, fromLocation, area );
            var portB = new FlowPipe.Port( to, toLocation, area );
            var pipe = new FlowPipe( portA, portB, length, area );
            builder.TryAddFlowObj( new object(), pipe );
            return pipe;
        }

        [Test, Description( "Verifies that a potential difference correctly initiates mass transfer from a full tank to an empty one." )]
        public void InitialFlow_OccursCorrectly_BasedOnPotential()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -10, 0 );
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, Vector3.zero, TestSubstances.Water, 1000 ); // Full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, Vector3.zero ); // Empty

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, -1, 0 ), 1.0f, 0.1f );

            var snapshot = builder.BuildSnapshot();

            // Act
            // Run a single step. The solver should calculate a flow rate and transfer mass.
            snapshot.Step( (float)DT );

            // Assert
            // We are now doing a black-box test. The internal flow rate value is less important
            // than the observable outcome: mass has been transferred.
            Assert.That( tankA.Outflow.GetMass(), Is.GreaterThan( 0 ), "Tank A's outflow should be positive." );
            Assert.That( tankB.Inflow.GetMass(), Is.GreaterThan( 0 ), "Tank B's inflow should be positive." );
            Assert.That( tankA.Outflow.GetMass(), Is.EqualTo( tankB.Inflow.GetMass() ).Within( 1e-9 ), "Outflow from A must equal inflow to B." );
        }

        [Test, Description( "Verifies a pump creates flow between two tanks at equal potential." )]
        public void PumpedFlowRate_IsCorrectlyCalculated_WithNoPotentialDifference()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero, TestSubstances.Water, 500 ); // Half full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero, TestSubstances.Water, 500 ); // Half full

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            var pipe = CreateAndAddPipe( builder, tankA, new Vector3( 0, -1, 0 ), tankB, new Vector3( 0, -1, 0 ), 1.0f );
            pipe.HeadAdded = 50.0; // Add 50 J/kg of head potential

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( (float)DT );

            // Assert
            double density = TestSubstances.Water.ReferenceDensity;
            double viscosity = TestSubstances.Water.GetViscosity( tankA.FluidState.Temperature, tankA.FluidState.Pressure );
            // On the first step, the solver assumes laminar flow to get an initial conductance.
            double expectedConductance = FlowEquations.GetMassConductance_Laminar( density, pipe.Area, pipe.Length, viscosity );
            double expectedUnrelaxedFlow = expectedConductance * pipe.HeadAdded;
            var relaxationFactor = GetInternalField<double>( snapshot, "_relaxationFactor" );

            Assert.That( snapshot.CurrentFlowRates[0], Is.EqualTo( expectedUnrelaxedFlow * relaxationFactor ).Within( 1e-9 ) );
            Assert.That( tankB.Inflow.GetMass(), Is.GreaterThan( 0 ), "Tank B should have received mass from the pump." );
        }

        [Test, Description( "Verifies that flow into a stiff (small, nearly full liquid tank) is damped more than flow into a non-stiff (large, gaseous) tank." )]
        public void StiffnessHandling_ProactiveDamping_ReducesFlowIntoStiffTank()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -10, 0 );

            var sourceTank = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, Vector3.zero, TestSubstances.Water, 10000 );
            var stiffSink = FlowNetworkTestHelper.CreateTestTank( 0.1, gravity, new Vector3( -5, 0, 0 ), TestSubstances.Water, 99.9 );
            var nonStiffSink = FlowNetworkTestHelper.CreateTestTank( 10.0, gravity, new Vector3( 5, 0, 0 ), TestSubstances.Air, 0.1 );

            builder.TryAddFlowObj( new object(), sourceTank );
            builder.TryAddFlowObj( new object(), stiffSink );
            builder.TryAddFlowObj( new object(), nonStiffSink );

            var pipeToStiff = CreateAndAddPipe( builder, sourceTank, new Vector3( -1, 0, 0 ), stiffSink, new Vector3( -4, 0, 0 ), 0.1f, 0.1f );
            var pipeToNonStiff = CreateAndAddPipe( builder, sourceTank, new Vector3( 1, 0, 0 ), nonStiffSink, new Vector3( 4, 0, 0 ), 0.1f, 0.1f );

            var snapshot = builder.BuildSnapshot();

            // Act
            snapshot.Step( (float)DT );
            stiffSink.ApplyFlows( DT );
            nonStiffSink.ApplyFlows( DT );

            // Assert
            int stiffPipeIndex = snapshot.Pipes.ToList().IndexOf( pipeToStiff );
            int nonStiffPipeIndex = snapshot.Pipes.ToList().IndexOf( pipeToNonStiff );

            double flowToStiff = snapshot.CurrentFlowRates[stiffPipeIndex];
            double flowToNonStiff = snapshot.CurrentFlowRates[nonStiffPipeIndex];

            Assert.That( flowToStiff, Is.GreaterThan( 0 ) );
            Assert.That( flowToNonStiff, Is.GreaterThan( 0 ) );
            Assert.That( Math.Abs( (float)flowToStiff ), Is.LessThan( Math.Abs( (float)flowToNonStiff ) * 0.1f ), "Flow into stiff tank should be significantly less due to proactive damping." );
        }

        [Test, Description( "Verifies that a high-conductance, oscillating pipe has its learned relaxation factor reduced to ensure stability." )]
        public void OscillationHandling_ReactiveDamping_ReducesRelaxationFactor()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, Vector3.zero, TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( 5, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = CreateAndAddPipe( builder, tankA, Vector3.right, tankB, Vector3.left, 0.001f, 0.5f ); // High conductance

            var snapshot = builder.BuildSnapshot();

            // Act
            // Run a few steps to allow the oscillation detector to kick in.
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            var learnedFactors = GetInternalField<double[]>( snapshot, "_pipeLearnedRelaxationFactors" );
            int pipeIndex = snapshot.Pipes.ToList().IndexOf( pipe );
            Assert.That( pipeIndex, Is.Not.EqualTo( -1 ) );
            Assert.That( learnedFactors[pipeIndex], Is.LessThan( 0.5 ), "High-conductance pipe should have its learned damping factor significantly reduced." );
        }

        [Test, Description( "Verifies that total mass is conserved in a complex network with multiple paths and loops." )]
        public void MassConservation_InComplexLoopingNetwork()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tanks = new FlowTank[4];
            for( int i = 0; i < 4; i++ )
            {
                tanks[i] = FlowNetworkTestHelper.CreateTestTank( 1.0, Vector3.zero, new Vector3( i * 5, 0, 0 ) );
                builder.TryAddFlowObj( new object(), tanks[i] );
            }
            tanks[0].Contents.Add( TestSubstances.Water, 1000 );
            double initialMass = tanks.Sum( t => t.Contents.GetMass() );

            CreateAndAddPipe( builder, tanks[0], Vector3.right, tanks[1], Vector3.left, 1.0f );
            CreateAndAddPipe( builder, tanks[1], Vector3.right, tanks[2], Vector3.left, 1.0f );
            CreateAndAddPipe( builder, tanks[2], Vector3.right, tanks[3], Vector3.left, 1.0f );
            CreateAndAddPipe( builder, tanks[3], Vector3.right, tanks[0], Vector3.left, 1.0f ); // Loop back
            CreateAndAddPipe( builder, tanks[0], Vector3.up, tanks[2], Vector3.up, 1.0f );     // Diagonal

            var snapshot = builder.BuildSnapshot();

            // Act
            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
                foreach( var t in tanks ) 
                    t.ApplyFlows( DT );
            }

            // Assert
            double finalMass = tanks.Sum( t => t.Contents.GetMass() );
            Assert.That( finalMass, Is.EqualTo( initialMass ).Within( 1e-9 ), "Total mass was not conserved in a complex network." );

            // Also check for equilibrium
            Assert.That( tanks[0].Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 0.01 ).Percent );
            Assert.That( tanks[1].Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 0.01 ).Percent );
            Assert.That( tanks[2].Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 0.01 ).Percent );
            Assert.That( tanks[3].Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 0.01 ).Percent );
        }

        [Test, Description( "Verifies that ullage pressure in a tank correctly increases the outflow rate of a liquid, demonstrating that pressure head is properly included in the potential calculation." )]
        public void UllagePressure_IncreasesLiquidOutflow()
        {
            // Arrange
            const double initialLiquidMass = 500;
            const double pressurantMass = 1.2; // ~1 atm in 0.5m^3 ullage
            const int simulationSteps = 50;

            // --- Scenario 1: Unpressurized ---
            var builderUnpressurized = new FlowNetworkBuilder();
            var tankA1 = FlowNetworkTestHelper.CreateTestTank( 2.0, Vector3.zero, Vector3.zero, TestSubstances.Water, initialLiquidMass );
            var tankB1 = FlowNetworkTestHelper.CreateTestTank( 2.0, Vector3.zero, new Vector3( 5, 0, 0 ) );
            builderUnpressurized.TryAddFlowObj( new object(), tankA1 );
            builderUnpressurized.TryAddFlowObj( new object(), tankB1 );
            CreateAndAddPipe( builderUnpressurized, tankA1, Vector3.right, tankB1, Vector3.left, 1.0f );
            var snapshotUnpressurized = builderUnpressurized.BuildSnapshot();

            // --- Scenario 2: Pressurized ---
            var builderPressurized = new FlowNetworkBuilder();
            var tankA2 = FlowNetworkTestHelper.CreateTestTank( 2.0, Vector3.zero, Vector3.zero, TestSubstances.Water, initialLiquidMass );
            tankA2.Contents.Add( TestSubstances.Air, pressurantMass ); // Add ullage gas
            var tankB2 = FlowNetworkTestHelper.CreateTestTank( 2.0, Vector3.zero, new Vector3( 5, 0, 0 ) );
            builderPressurized.TryAddFlowObj( new object(), tankA2 );
            builderPressurized.TryAddFlowObj( new object(), tankB2 );
            CreateAndAddPipe( builderPressurized, tankA2, Vector3.right, tankB2, Vector3.left, 1.0f );
            var snapshotPressurized = builderPressurized.BuildSnapshot();

            // Act
            for( int i = 0; i < simulationSteps; i++ )
            {
                snapshotUnpressurized.Step( (float)DT );
                tankA1.ApplyFlows( DT );
                tankB1.ApplyFlows( DT );

                snapshotPressurized.Step( (float)DT );
                tankA2.ApplyFlows( DT );
                tankB2.ApplyFlows( DT );
            }

            // Assert
            double massLost_Unpressurized = initialLiquidMass - tankA1.Contents[TestSubstances.Water];
            double massLost_Pressurized = initialLiquidMass - tankA2.Contents[TestSubstances.Water];

            Assert.That( massLost_Pressurized, Is.GreaterThan( massLost_Unpressurized ), "The pressurized tank should have a higher liquid outflow rate than the unpressurized one." );
        }

        [Test, Description( "Characterizes the solver's ability to reach and hold a stable hydrostatic equilibrium in a U-Tube configuration." )]
        public void HydrostaticEquilibrium_InUTube_IsReachedAndHeld()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var gravity = new Vector3( 0, -10, 0 );
            var tankL = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( -2, 0, 0 ) );
            var tankR = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 2, 0, 0 ) );

            tankL.Contents.Add( TestSubstances.Water, 500 ); // Fill left tank halfway

            builder.TryAddFlowObj( new object(), tankL );
            builder.TryAddFlowObj( new object(), tankR );
            var pipe = CreateAndAddPipe( builder, tankL, new Vector3( -2, -1, 0 ), tankR, new Vector3( 2, -1, 0 ), 1.0f ); // Connect at the bottom

            var snapshot = builder.BuildSnapshot();

            // Act
            // Run for enough steps to allow the levels to equalize.
            for( int i = 0; i < 500; i++ )
            {
                snapshot.Step( (float)DT );
                tankL.ApplyFlows( DT );
                tankR.ApplyFlows( DT );
            }

            // Assert: Equilibrium should be reached.
            Assert.That( tankL.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 5.0 ), "Left tank should settle at half the total mass." );
            Assert.That( tankR.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 5.0 ), "Right tank should settle at half the total mass." );

            int pipeIndex = snapshot.Pipes.ToList().IndexOf( pipe );
            Assert.That( snapshot.CurrentFlowRates[pipeIndex], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow rate should be effectively zero at equilibrium." );

            // Act: Run for more steps to ensure equilibrium is held.
            for( int i = 0; i < 100; i++ )
            {
                snapshot.Step( (float)DT );
                tankL.ApplyFlows( DT );
                tankR.ApplyFlows( DT );
            }

            // Assert: State should not have changed.
            Assert.That( tankL.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 5.0 ), "Left tank mass should remain stable after reaching equilibrium." );
            Assert.That( tankR.Contents.GetMass(), Is.EqualTo( 250.0 ).Within( 5.0 ), "Right tank mass should remain stable after reaching equilibrium." );
            Assert.That( snapshot.CurrentFlowRates[pipeIndex], Is.EqualTo( 0.0 ).Within( 1e-6 ), "Flow rate should remain zero after reaching equilibrium." );
        }

        [TestCase( 10.0, 0.02, Description = "Standard timestep" )]
        [TestCase( 10.0, 0.04, Description = "Larger timestep (e.g. 2x timewarp)" )]
        [Description( "Characterizes the solver's numerical stability by asserting that the final simulation state is consistent across different timestep sizes for the same total duration." )]
        public void SimulationResult_IsReasonablyIndependentOfTimestep( double totalDuration, double dt )
        {
            // This test runs the same scenario twice with different timesteps and compares the final mass distribution.
            // A stable solver should produce very similar results.

            Func<double, double> runSimulation = ( timestep ) =>
            {
                // Arrange
                var builder = new FlowNetworkBuilder();
                var gravity = new Vector3( 0, -10, 0 );
                var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, Vector3.zero, TestSubstances.Water, 1000 );
                var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, gravity, new Vector3( 5, 0, 0 ) );
                builder.TryAddFlowObj( new object(), tankA );
                builder.TryAddFlowObj( new object(), tankB );
                CreateAndAddPipe( builder, tankA, Vector3.right, tankB, Vector3.left, 10.0f, 0.01f );
                var snapshot = builder.BuildSnapshot();

                // Act
                int steps = (int)(totalDuration / timestep);
                for( int i = 0; i < steps; i++ )
                {
                    snapshot.Step( (float)timestep );
                    tankA.ApplyFlows( timestep );
                    tankB.ApplyFlows( timestep );
                }

                // Return final mass of one tank for comparison
                return tankA.Contents.GetMass();
            };

            // Run with a baseline small dt
            double finalMass1 = runSimulation( 0.01 );

            // Run with the specified test dt
            double finalMass2 = runSimulation( dt );

            // Assert
            // A small deviation is expected due to numerical integration differences. 1% is a reasonable tolerance.
            Assert.That( finalMass2, Is.EqualTo( finalMass1 ).Within( finalMass1 * 0.01 ), $"Final mass differs significantly with dt={dt}. Expected ~{finalMass1}, but got {finalMass2}." );
        }
    }
}
