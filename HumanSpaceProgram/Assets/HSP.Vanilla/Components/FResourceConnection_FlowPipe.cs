using HSP.ResourceFlow;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceConnection_FlowPipe : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea { get; set; } = 0.1f;
        public ResourceInlet FromInlet { get; set; }
        public ResourceInlet ToInlet { get; set; }

        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
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

            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        public virtual bool IsValid( FlowNetworkSnapshot snapshot )
        {
#warning TODO - implement.
            return false;
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
                .WithMember( "cross_section_area", o => o.CrossSectionArea );
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