using UnityEngine;

namespace HSP.ResourceFlow
{
    public enum BuildFlowResult
    {
        Finished,
        Retry,
        Failure
    }

    public interface IBuildsFlowNetwork
    {
        BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c );
        void ApplySnapshot( FlowNetworkSnapshot snapshot );
    }

    public class FResourceContainer_FlowTank : MonoBehaviour, IBuildsFlowNetwork
    {
        public Vector3[] triangulationPositions; // initial pos for triangulation.
        public ResourceInlet[] inlets;
        public SubstanceStateCollection contents;

        public FlowTank tank;


        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // build the tank?

            // only add if something attaches to the tank that is open.
#warning TODO - add offsets due to transform.
            c.AddTank( tank );
            return BuildFlowResult.Finished;
        }
        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the tank.
        }


        void Start()
        {
            tank = new FlowTank();
            tank.SetNodes( triangulationPositions, inlets );
        }

        void FixedUpdate()
        {
            tank.DistributeFluids();
        }
    }
}