using HSP.ResourceFlow;
using HSP_Tests;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkDynamicTests
    {
        private const double DT = 0.02;
        private static readonly Vector3 GRAVITY = new Vector3( 0, -10, 0 );

        /// <summary>
        /// A mock IBuildsFlowNetwork component used to test invalidation.
        /// Its IsValid implementation allows us to trigger a rebuild, and its
        /// BuildFlowNetwork method correctly removes its old pipe if disabled.
        /// </summary>
        private class MockPipeProvider : IBuildsFlowNetwork
        {
            public bool IsEnabled = true;
            public FlowTank TankA, TankB;

            private FlowPipe _pipe;

            public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
            {
                // If we are being rebuilt, assume we must remove old contributions first.
                if( _pipe != null )
                {
                    c.TryRemoveFlowObj( _pipe );
                    _pipe = null;
                }

                if( IsEnabled )
                {
                    var portA = new FlowPipe.Port( (IResourceConsumer)TankA, Vector3.zero, 0.1f );
                    var portB = new FlowPipe.Port( (IResourceConsumer)TankB, Vector3.zero, 0.1f );
                    _pipe = new FlowPipe( portA, portB, 1.0, 0.1 );
                    c.TryAddFlowObj( this, _pipe );
                }
                return BuildFlowResult.Finished;
            }

            public bool IsValid( FlowNetworkSnapshot snapshot )
            {
                // To trigger a rebuild, we report invalid if our enabled state
                // doesn't match the network's current state (i.e., if the pipe exists when it shouldn't).
                bool isPipeInNetwork = _pipe != null && snapshot.Pipes.Contains( _pipe );
                return IsEnabled == isPipeInNetwork;
            }

            public void SynchronizeState( FlowNetworkSnapshot snapshot ) { }
            public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
        }

        [Test]
        public void RuntimePipeAddition___WhenTransactionApplied___FlowStarts()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            var snapshot = builder.BuildSnapshot();

            // Act: Run for a bit without a pipe
            for( int i = 0; i < 50; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert: No flow occurred
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0.0 ), "Mass should not transfer before pipe is added." );

            // Act: Add a pipe via a transaction
            var transaction = new FlowNetworkBuilder();
            FlowNetworkTestHelper.CreateAndAddPipe( transaction, tankA, new Vector3( 0, 9, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0f );
            snapshot.ApplyTransaction( transaction );

            // Act: Run for more steps with the pipe
            for( int i = 0; i < 100; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert: Flow has started
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 1.0 ), "Mass should transfer after pipe is added." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
        }

        [Test]
        public void RuntimePipeRemoval___WhenTransactionApplied___FlowStops()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            var pipe = FlowNetworkTestHelper.CreateAndAddPipe( builder, tankA, new Vector3( 0, 9, 0 ), tankB, new Vector3( 0, 1, 0 ), 1.0f );

            var snapshot = builder.BuildSnapshot();

            // Act: Run for a bit with the pipe
            for( int i = 0; i < 50; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert: Flow occurred
            double massAfterInitialFlow = tankB.Contents.GetMass();
            Assert.That( massAfterInitialFlow, Is.GreaterThan( 1.0 ), "Mass should transfer while pipe exists." );

            // Act: Remove the pipe via a transaction
            var transaction = new FlowNetworkBuilder();
            transaction.TryRemoveFlowObj( pipe );
            snapshot.ApplyTransaction( transaction );

            // Act: Run for more steps without the pipe
            for( int i = 0; i < 100; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert: Flow has stopped
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( massAfterInitialFlow ).Within( 1e-9 ), "Mass should not transfer after pipe is removed." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
        }

        [Test]
        public void ComponentInvalidation___WhenComponentBecomesInvalid___PipeIsRemoved()
        {
            // Arrange
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, new Vector3( 0, 10, 0 ), TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, GRAVITY, Vector3.zero );
            var mockPipeProvider = new MockPipeProvider { IsEnabled = true, TankA = tankA, TankB = tankB };

            // Simulate initial network build from components
            var builder = new FlowNetworkBuilder();
            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );
            mockPipeProvider.BuildFlowNetwork( builder );
            var snapshot = builder.BuildSnapshot();

            // Act: Run for a bit with the pipe enabled
            for( int i = 0; i < 50; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert: Flow occurred
            double massAfterInitialFlow = tankB.Contents.GetMass();
            Assert.That( massAfterInitialFlow, Is.GreaterThan( 1.0 ), "Mass should transfer while component is valid." );

            // Act: Invalidate the component and simulate the manager's rebuild logic
            mockPipeProvider.IsEnabled = false;

            var invalidComponents = new List<IBuildsFlowNetwork>();
            snapshot.GetInvalidComponents( invalidComponents ); // Snapshot detects the invalid component
            Assert.That( invalidComponents, Does.Contain( mockPipeProvider ) );

            var transaction = new FlowNetworkBuilder();
            foreach( var component in invalidComponents )
            {
                component.BuildFlowNetwork( transaction ); // Rebuild invalid component, which removes its pipe
            }
            snapshot.ApplyTransaction( transaction );

            // Act: Run for more steps
            for( int i = 0; i < 100; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert: Flow has stopped
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( massAfterInitialFlow ).Within( 1e-9 ), "Mass should not transfer after component is invalidated." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-6 ) );
        }
    }
}
