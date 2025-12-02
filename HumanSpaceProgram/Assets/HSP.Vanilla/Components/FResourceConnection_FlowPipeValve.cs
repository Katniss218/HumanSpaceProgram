using HSP.ResourceFlow;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceConnection_FlowPipeValve : FResourceConnection_FlowPipe
    {
        public float PercentOpen { get; set; } = 0.0f; // 0..1

        // --- State for Partial Rebuilds ---
        private FlowPipe _cachedPipe;
        private bool _wasOpenLastBuild;

        public override BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            bool isOpen = PercentOpen >= 1e-3f;

            if( isOpen )
            {
                // We should be in the network.
                // If we were previously closed, we need to create and add a new pipe.
                if( !_wasOpenLastBuild )
                {
                    if( FromInlet == null || ToInlet == null )
                        return BuildFlowResult.Finished; // Can't build if disconnected.

                    if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
                    {
                        return BuildFlowResult.Retry;
                    }

                    _cachedPipe = new FlowPipe( flowEnd1, flowEnd2, conductance: Conductance * PercentOpen );
                    c.TryAddFlowObj( this, _cachedPipe );
                }
            }
            else
            {
                // We should NOT be in the network.
                // If we were previously open, we must remove our pipe.
                if( _wasOpenLastBuild && _cachedPipe != null )
                {
                    c.TryRemoveFlowObj( _cachedPipe );
                    _cachedPipe = null;
                }
            }

            _wasOpenLastBuild = isOpen;
            return BuildFlowResult.Finished;
        }

        public override bool IsValid( FlowNetworkSnapshot snapshot )
        {
            // The network structure is invalid only if our open/closed state has changed.
            bool isOpen = PercentOpen >= 1e-3f;
            return isOpen == _wasOpenLastBuild;
        }

        public override void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // If our pipe exists in the simulation, update its conductance.
            // This handles changes in PercentOpen that don't cross the open/close threshold.
            if( _cachedPipe != null )
            {
                _cachedPipe.Conductance = this.PercentOpen;
            }
        }

        [MapsInheritingFrom( typeof( FResourceConnection_FlowPipeValve ) )]
        public static SerializationMapping FResourceConnection_FlowPipeValveMapping()
        {
            return new MemberwiseSerializationMapping<FResourceConnection_FlowPipeValve>()
                .WithMember( "percent_open", o => o.PercentOpen );
        }
    }

    /*
    public class FResourceConnector_FlowPipeCheckValve : FResourceConnector_FlowPipe
    {

    }
    public class FResourceConnector_FlowPipeReliefValve : FResourceConnector_FlowPipe
    {

    }*/
}