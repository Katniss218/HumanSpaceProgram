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

        private FlowPipe _cachedPumpPipe;

        public override BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            if( FromInlet == null || ToInlet == null )
            {
                if( _cachedPumpPipe != null )
                {
                    c.TryRemoveFlowObj( _cachedPumpPipe );
                    _cachedPumpPipe = null;
                }
                return BuildFlowResult.Finished;
            }

            // Try to get the simulation ports for the inlets
            if( !c.TryGetFlowObj( FromInlet, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( ToInlet, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            // Create pipe with added Head Pressure
            _cachedPumpPipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea, headAdded: PressureHead );
            c.TryAddFlowObj( this, _cachedPumpPipe );
            return BuildFlowResult.Finished;
        }

        public override void SynchronizeState( FlowNetworkSnapshot snapshot )
        {
            // If our pipe exists in the simulation, update its head pressure.
            if( _cachedPumpPipe != null )
            {
                _cachedPumpPipe.HeadAdded = this.PressureHead;
            }
        }

        [MapsInheritingFrom( typeof( FResourceConnection_FlowPipePump ) )]
        public static SerializationMapping FResourceConnection_FlowPipePumpMapping()
        {
            return new MemberwiseSerializationMapping<FResourceConnection_FlowPipePump>()
                .WithMember( "pressure_head", o => o.PressureHead );
        }
    }
}