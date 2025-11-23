using HSP.ResourceFlow;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceConnector_FlowPipeValve : FResourceConnection_FlowPipe
    {
        public float PercentOpen { get; set; } = 0.0f; // 0..1

        public override BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // If the valve is closed, we do not create the simulation pipe, effectively blocking flow.
            if( PercentOpen < 1e-3f )
                return BuildFlowResult.Finished;

            if( FromInlet == null || ToInlet == null )
                return BuildFlowResult.Finished;

            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            // Create pipe with specific conductance
            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea, conductance: PercentOpen );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        [MapsInheritingFrom( typeof( FResourceConnector_FlowPipeValve ) )]
        public static SerializationMapping FResourceConnector_FlowPipeValveMapping()
        {
            return new MemberwiseSerializationMapping<FResourceConnector_FlowPipeValve>()
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