using HSP.ResourceFlow;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceConnection_FlowPipe : MonoBehaviour, IBuildsFlowNetwork
    {
        /// <summary>
        /// A multiplier for conductance, representing pipe type or losses from bends. [unitless]
        /// Final conductance is ConductivityFactor * Area / Length.
        /// </summary>
        public double ConductivityFactor { get; set; } = 1.0;

        /// <summary>
        /// The length of the pipe, in [m].
        /// </summary>
        public double Length { get; set; } = 1.0;

        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }

        public List<IPipeModifier> Modifiers { get; private set; } = new List<IPipeModifier>();

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

                double area = Math.Min( FromInlet.NominalArea, ToInlet.NominalArea );
                double length = Math.Max( Length, 0.01 ); // Min length 1cm

                _cachedPipe = new FlowPipe( flowEnd1, flowEnd2, length, area );
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
            if( _cachedPipe != null )
            {
                // The solver is now responsible for calculating the base physical conductance.
                // This method is now only for applying modifiers like pumps or valves.
                _cachedPipe.HeadAdded = 0.0;

                // Apply all modifiers, which can alter conductance or add head.
                foreach( var modifier in Modifiers )
                {
                    modifier.Apply( _cachedPipe );
                }
            }
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
                .WithMember( "conductivity_factor", o => o.ConductivityFactor )
                .WithMember( "length", o => o.Length )
                .WithMember( "modifiers", o => o.Modifiers );
        }
    }
}


/*
public class FResourceConnector_FlowPipeCheckValve : FResourceConnector_FlowPipe
{

}
public class FResourceConnector_FlowPipeReliefValve : FResourceConnector_FlowPipe
{

}*/