using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FResourceConnector_FlowPipe : MonoBehaviour, IBuildsFlowNetwork
    {
        public float CrossSectionArea = 0.1f;
#warning TODO - store the object that it connects to?
        public ResourceInlet end1;
        public ResourceInlet end2;

        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // build the actual pipe. needs to reference the correct inlets/outlets.

            // only add if valve open, etc.
            // inlets/outlets are to the tank which is built by the builder.
            // ordering issue, tanks must be built before pipes.
            if( !c.TryGetFlowObj( end1, out FlowPipe.Port flowEnd1 ) || !c.TryGetFlowObj( end2, out FlowPipe.Port flowEnd2 ) )
            {
                return BuildFlowResult.Retry;
            }

            FlowPipe pipe = new FlowPipe( flowEnd1, flowEnd2, CrossSectionArea );
            c.TryAddFlowObj( this, pipe );
            return BuildFlowResult.Finished;
        }

        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the pipe.
        }
    }

    /*public class FResourceConnector_FlowPipePump : FResourceConnector_FlowPipe
    {

    }
    public class FResourceConnector_FlowPipeValve : FResourceConnector_FlowPipe
    {

    }
    public class FResourceConnector_FlowPipeCheckValve : FResourceConnector_FlowPipe
    {

    }
    public class FResourceConnector_FlowPipeReliefValve : FResourceConnector_FlowPipe
    {

    }*/
}