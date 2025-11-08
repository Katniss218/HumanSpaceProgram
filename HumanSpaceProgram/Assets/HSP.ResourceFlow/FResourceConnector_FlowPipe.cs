using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FResourceConnector_FlowPipe : MonoBehaviour, IBuildsFlowNetwork
    {
        public ResourceInlet end1;
        public ResourceInlet end2;

        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // build the actual pipe. needs to reference the correct inlets/outlets.

            // only add if valve open, etc.
            // inlets/outlets are to the tank which is built by the builder.
            // ordering issue, tanks must be built before pipes.
            if( c.TryGetOwner( end1.owner, out  ) || c.TryGetOwner( end2.owner, out ) == null )
            {
                return BuildFlowResult.Retry;
            }
#warning TODO - add offsets due to transform.

            c.AddPipe( pipe );
            return BuildFlowResult.Finished;
        }

        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the tank.
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