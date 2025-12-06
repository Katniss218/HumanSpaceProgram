using HSP.ResourceFlow;
using HSP.Time;
using UnityEngine;

namespace HSP_Tests_PlayMode.ResourceFlow
{
    public static class FlowNetworkTestHelper
    {
        public static (GameObject manager, TimeManager timeManager, AssertMonoBehaviour assertMonoBeh) CreateTestScene()
        {
            GameObject manager = new GameObject( "TestManager" );
            TimeManager timeManager = manager.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );
            var assertMonoBeh = manager.AddComponent<AssertMonoBehaviour>();

            return (manager, timeManager, assertMonoBeh);
        }
    }

    public sealed class MockFlowTankWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public FlowTank Tank { get; set; }
        public ResourceInlet[] Inlets { get; set; }

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            c.TryAddFlowObj( this, Tank );
            foreach( var inlet in Inlets )
            {
                FlowPipe.Port flowInlet = new FlowPipe.Port( (IResourceConsumer)Tank, inlet.LocalPosition, inlet.NominalArea );
                c.TryAddFlowObj( inlet, flowInlet );
            }

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            return false;
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // The FlowNetworkSnapshot now calls ApplyFlows() on the simulation objects (FlowTanks) directly,
            // which handles resource transfer and cache invalidation. This wrapper's role is to bridge
            // the simulation and the Unity component. Since tests inspect the simulation object (Tank)
            // directly, there's no state to copy back. This method is empty for testing purposes.
        }

        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // In a real component, this method would push state (like physics vectors) into the simulation object.
            // For these tests, physics properties like FluidAcceleration are set directly on the Tank object
            // during test setup, so this can be left empty.
        }
    }

    public sealed class MockFlowPipeWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea { get; set; } = 0.1f;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null )
                return BuildFlowResult.Finished;
            if( ToInlet == null )
                return BuildFlowResult.Finished;

            // only add if valve open, etc.

            // inlets are part of the tank which is built by the builder.
            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, conductance: 0.01 );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot )
        {
            return false;
        }

        public void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
        }

        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // A basic static pipe has no dynamic state to synchronize from a Unity component.
            // A dynamic component like a pump would override this to update its HeadAdded property.
        }
    }

    /// <summary>
    /// A custom pipe wrapper for the oscillation test that allows setting a high conductance value.
    /// It follows the same (apparently flawed) builder pattern as existing components to ensure compatibility.
    /// </summary>
    public sealed class HighConductancePipeWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea { get; set; } = 1.0f;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }
        public double Conductance { get; set; } = 5000.0; // ridiculously huge conductance for testing.

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null || ToInlet == null )
                return BuildFlowResult.Finished;

            // This call pattern is suspect but consistent with other components.
            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, conductance: this.Conductance );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot ) => true;
        public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // This mock pipe has no dynamic state to synchronize.
        }
    }

    public sealed class MockEngineWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public GenericConsumer Consumer { get; private set; }
        public ResourceInlet Inlet { get; set; }

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( Inlet == null )
            {
                return BuildFlowResult.Finished;
            }

            Consumer = new GenericConsumer();
            Consumer.Demand = 100.0; // 100 kg/s demand for testing.
            Consumer.IsEnabled = true;
            c.TryAddFlowObj( this, Consumer );

            FlowPipe.Port flowPort = new FlowPipe.Port( Consumer, Inlet.LocalPosition, Inlet.NominalArea );
            c.TryAddFlowObj( Inlet, flowPort );

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot ) => true;
        public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
        public void SynchronizeState( FlowNetworkSnapshot snapshot ) { }
    }

    public sealed class MockPumpWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea { get; set; } = 0.1f;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }
        public double HeadAdded { get; set; } = 100.0;

        private FlowPipe _cachedPipe;

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null || ToInlet == null )
                return BuildFlowResult.Finished;

            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            _cachedPipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea, headAdded: HeadAdded );
            c.TryAddFlowObj( this, _cachedPipe );
            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot ) => true;
        public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
        public void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            if( _cachedPipe != null )
            {
                _cachedPipe.HeadAdded = this.HeadAdded;
            }
        }
    }

    public sealed class MockValveWrapper : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea { get; set; } = 0.1f;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }
        public bool IsOpen { get; set; }

        private FlowPipe _cachedPipe;

        public BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            _cachedPipe = null;

            if( !IsOpen || FromInlet == null || ToInlet == null )
            {
                return BuildFlowResult.Finished;
            }

            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            _cachedPipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea );
            c.TryAddFlowObj( this, _cachedPipe );

            return BuildFlowResult.Finished;
        }

        public bool IsValid( FlowNetworkSnapshot snapshot ) => true;
        public void ApplySnapshot( FlowNetworkSnapshot snapshot ) { }
        public void SynchronizeState( FlowNetworkSnapshot snapshot ) { }
    }
}
