using HSP.ResourceFlow;
using NUnit.Framework;
using HSP_Tests;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class FlowNetworkDynamicTests
    {
        private const double DT = 0.02;

        /// <summary>
        /// A mock IBuildsFlowNetwork component that can be enabled or disabled to test network rebuilds.
        /// </summary>
        private class ToggleablePipeComponent : IBuildsFlowNetwork
        {
            public bool IsEnabled = true;
            private bool _wasEnabledLastBuild = true;

            private readonly IResourceConsumer _from;
            private readonly IResourceConsumer _to;
            private readonly double _length;
            private readonly object _owner = new object();

            private FlowPipe _pipe;

            public ToggleablePipeComponent( IResourceConsumer from, IResourceConsumer to, double length )
            {
                _from = from;
                _to = to;
                _length = length;
            }

            public bool IsValid( FlowNetworkSnapshot snapshot )
            {
                return IsEnabled == _wasEnabledLastBuild;
            }



            public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
            {
                _wasEnabledLastBuild = IsEnabled;

                // If it was in the network but is now disabled, remove it.
                if( !IsEnabled && _pipe != null )
                {
                    c.TryRemoveFlowObj( _pipe );
                    _pipe = null;
                    return BuildFlowResult.Finished;
                }

                // If it should be in the network but isn't yet, add it.
                if( IsEnabled && _pipe == null )
                {
                    var portA = new FlowPipe.Port( _from, Vector3.zero, 0.1f );
                    var portB = new FlowPipe.Port( _to, Vector3.zero, 0.1f );

                    _pipe = new FlowPipe( portA, portB, _length, 0.1f );
                    c.TryAddFlowObj( _owner, _pipe );
                }

                return BuildFlowResult.Finished;
            }

            public void SynchronizeState( FlowNetworkSnapshot snapshot ) { }
            public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
        }

        [Test, Description( "Verifies that adding a pipe to a previously disconnected system initiates flow." )]
        public void AddPipe_StartsFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( TestSubstances.Water, 1000 ); // Full
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 2, 0, 0 ) );

            var ownerA = new object();
            var ownerB = new object();
            builder.TryAddFlowObj( ownerA, tankA );
            builder.TryAddFlowObj( ownerB, tankB );

            // Start with a snapshot that has no pipes.
            var snapshot = builder.BuildSnapshot();

            // Act 1: Simulate a few steps to confirm no flow occurs.
            for( int i = 0; i < 5; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert 1: No flow should have occurred.
            Assert.That( tankA.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 1e-9 ), "Mass should not change before pipe is added." );
            Assert.That( tankB.Contents.GetMass(), Is.EqualTo( 0.0 ).Within( 1e-9 ), "Mass should not change before pipe is added." );

            // Act 2: Create a transaction to add a pipe and apply it.
            var transaction = new FlowNetworkBuilder();
            var portA = new FlowPipe.Port( (IResourceConsumer)tankA, new Vector3( 1, 0, 0 ), 0.1f );
            var portB = new FlowPipe.Port( (IResourceConsumer)tankB, new Vector3( 1, 0, 0 ), 0.1f );
            var pipe = new FlowPipe( portA, portB, 1.0, 0.1f );
            transaction.TryAddFlowObj( new object(), pipe );
            snapshot.ApplyTransaction( transaction );

            // Act 3: Simulate a few more steps.
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert 3: Flow should now have occurred.
            Assert.That( tankA.Contents.GetMass(), Is.LessThan( 1000.0 ), "Tank A should have lost mass after pipe was added." );
            Assert.That( tankB.Contents.GetMass(), Is.GreaterThan( 0.0 ), "Tank B should have gained mass after pipe was added." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.01 ).Percent, "Total mass must be conserved." );
        }

        [Test, Description( "Verifies that removing a pipe from an active system correctly halts the flow." )]
        public void RemovePipe_StopsFlow()
        {
            // Arrange
            var builder = new FlowNetworkBuilder();
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 2, 0, 0 ) );

            builder.TryAddFlowObj( new object(), tankA );
            builder.TryAddFlowObj( new object(), tankB );

            var portA = new FlowPipe.Port( (IResourceConsumer)tankA, new Vector3( 1, 0, 0 ), 0.1f );
            var portB = new FlowPipe.Port( (IResourceConsumer)tankB, new Vector3( 1, 0, 0 ), 0.1f );
            var pipe = new FlowPipe( portA, portB, 1.0, 0.1f );
            builder.TryAddFlowObj( new object(), pipe );

            var snapshot = builder.BuildSnapshot();

            // Act 1: Simulate to confirm flow is active.
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert 1: Flow is active.
            double massB_beforeRemoval = tankB.Contents.GetMass();
            Assert.That( massB_beforeRemoval, Is.GreaterThan( 0.0 ), "Tank B should have received mass while connected." );

            // Act 2: Create a transaction to remove the pipe and apply it.
            var transaction = new FlowNetworkBuilder();
            transaction.TryRemoveFlowObj( pipe );
            snapshot.ApplyTransaction( transaction );

            // Act 3: Simulate more steps.
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert 3: Flow should have stopped.
            double massB_afterRemoval = tankB.Contents.GetMass();
            Assert.That( massB_afterRemoval, Is.EqualTo( massB_beforeRemoval ).Within( 0.01 ).Percent, "Tank B mass should not change after pipe is removed." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.01 ).Percent, "Total mass must be conserved." );
        }

        [Test, Description( "Verifies that a component becoming invalid correctly triggers a network rebuild that removes its pipe." )]
        public void ComponentInvalidation_TriggersRebuildAndStopsFlow()
        {
            // Arrange
            var tankA = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), Vector3.zero );
            tankA.Contents.Add( TestSubstances.Water, 1000 );
            var tankB = FlowNetworkTestHelper.CreateTestTank( 1.0, new Vector3( 0, -10, 0 ), new Vector3( 2, 0, 0 ) );
            var toggleablePipe = new ToggleablePipeComponent( tankA, tankB, 1.0 );

            var builder = new FlowNetworkBuilder();
            builder.TryAddFlowObj( tankA, tankA ); // Use tanks as their own owners for simplicity
            builder.TryAddFlowObj( tankB, tankB );

            // The component adds its own pipe to the builder
            toggleablePipe.BuildFlowNetwork( builder );

            // Pass the component to the snapshot so its validity can be checked.
            var components = new IBuildsFlowNetwork[] { toggleablePipe };
            var snapshot = new FlowNetworkSnapshot( null, builder.Owner, components, builder.Producers.ToList(), builder.Consumers.ToList(), builder.Pipes.ToList() );

            // Act 1: Simulate a few steps with the pipe enabled.
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }
            double massB_beforeToggle = tankB.Contents.GetMass();
            Assert.That( massB_beforeToggle, Is.GreaterThan( 0.0 ), "Flow should occur when pipe is enabled." );

            // Act 2: Disable the component, making it invalid.
            toggleablePipe.IsEnabled = false;

            // Act 3: Simulate the manager's invalidation-check-and-rebuild loop.
            var invalidComponents = new List<IBuildsFlowNetwork>();
            snapshot.GetInvalidComponents( invalidComponents );

            Assert.That( invalidComponents, Does.Contain( toggleablePipe ), "GetInvalidComponents should have detected the state change." );
            Assert.That( invalidComponents.Count, Is.EqualTo( 1 ) );

            var transaction = new FlowNetworkBuilder();
            foreach( var invalid in invalidComponents )
            {
                invalid.BuildFlowNetwork( transaction );
            }
            snapshot.ApplyTransaction( transaction );

            // Act 4: Simulate a few more steps.
            for( int i = 0; i < 10; i++ )
            {
                snapshot.Step( (float)DT );
            }

            // Assert
            double massB_afterToggle = tankB.Contents.GetMass();
            Assert.That( massB_afterToggle, Is.EqualTo( massB_beforeToggle ).Within( 0.01 ).Percent, "Mass should not change after component is disabled and network rebuilt." );
            Assert.That( tankA.Contents.GetMass() + tankB.Contents.GetMass(), Is.EqualTo( 1000.0 ).Within( 0.01 ).Percent, "Total mass must be conserved." );
        }
    }
}
