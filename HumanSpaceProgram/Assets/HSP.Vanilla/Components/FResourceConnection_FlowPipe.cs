using HSP.ResourceFlow;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceConnection_FlowPipe : MonoBehaviour, IBuildsFlowNetwork
    {
        public double Conductance { get; set; } = 1.0;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }

        // --- State for Partial Rebuilds ---
        private FlowPipe _cachedPipe;
        private bool _isInNetwork;

        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null || ToInlet == null )
            {
                // If we were previously in the network, remove ourselves.
                if( _isInNetwork && _cachedPipe != null )
                {
                    c.TryRemoveFlowObj( _cachedPipe );
                    _cachedPipe = null;
                    _isInNetwork = false;
                }
                return BuildFlowResult.Finished;
            }

            // Standard pipe is always "on".
            // If we were already in the network, no change is needed.
            // If not, we add ourselves.
            if( !_isInNetwork )
            {
                if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
                {
                    return BuildFlowResult.Retry;
                }

                _cachedPipe = new FlowPipe( flowEnd1, flowEnd2, Conductance );
                c.TryAddFlowObj( this, _cachedPipe );
                _isInNetwork = true;
            }

            return BuildFlowResult.Finished;
        }

        public virtual bool IsValid( FlowNetworkSnapshot snapshot )
        {
            // A basic static pipe is always valid unless its connections are removed.
            // The builder will handle removing it if FromInlet/ToInlet become null.
            bool shouldBeInNetwork = FromInlet != null && ToInlet != null;
            if( _isInNetwork != shouldBeInNetwork )
            {
                return false; // Our state (in vs out of network) has changed.
            }
            return true;
        }

        public virtual void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // Base pipe has no dynamic state to synchronize.
            // Derived classes (like pumps) will override this.
        }

        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the pipe.
        }


        [MapsInheritingFrom( typeof( FResourceConnection_FlowPipe ) )]
        public static SerializationMapping FResourceConnection_FlowPipeMapping()
        {
            return new MemberwiseSerializationMapping<FResourceConnection_FlowPipe>()
                .WithMember( "from_inlet", ObjectContext.Ref, o => o.FromInlet )
                .WithMember( "to_inlet", ObjectContext.Ref, o => o.ToInlet )
                .WithMember( "conductance", o => o.Conductance );
        }
    }
}