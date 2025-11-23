using HSP.ResourceFlow;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class FResourceConnection_FlowPipePump : FResourceConnection_FlowPipe
    {
        /// <summary>
        /// Pressure added to the flow in [Pa]. 
        /// Positive pushes from FromInlet to ToInlet.
        /// </summary>
        public float PressureHead { get; set; } = 10000f;

        public override BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null || ToInlet == null )
                return BuildFlowResult.Finished;

            // Try to get the simulation ports for the inlets
            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            // Create pipe with added Head Pressure
            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea, headAdded: PressureHead );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        [MapsInheritingFrom( typeof( FResourceConnection_FlowPipePump ) )]
        public static SerializationMapping FResourceConnection_FlowPipePumpMapping()
        {
            return new MemberwiseSerializationMapping<FResourceConnection_FlowPipePump>()
                .WithMember( "pressure_head", o => o.PressureHead );
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