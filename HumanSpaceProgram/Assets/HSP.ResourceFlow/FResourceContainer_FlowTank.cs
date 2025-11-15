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
        bool IsValid( FlowNetworkSnapshot snapshot );
        void ApplySnapshot( FlowNetworkSnapshot snapshot );
    }

    public class FResourceContainer_FlowTank : MonoBehaviour, IBuildsFlowNetwork
    {
        public Vector3[] triangulationPositions; // initial pos for triangulation.
        public ResourceInlet[] inlets;
        public SubstanceStateCollection contents;

        FlowTank _cachedTank;


        public virtual BuildFlowResult BuildFlowNetwork( FlowNetworkBuilder c )
        {
            // build the tank?

            // only add if something attaches to the tank that is open.
            if( _cachedTank == null )
            {
                // make tank.
                _cachedTank = new FlowTank();
                _cachedTank.SetNodes( triangulationPositions, inlets );
                _cachedTank.Contents;
            }

            // TODO - validate that something is connected to it. We can use a similar 'connection' structure to control inputs.
            _cachedTank.DistributeFluids();
            c.TryAddFlowObj( this, _cachedTank );
            foreach( var inlet in inlets )
            {
#warning TODO - add correct offsets due to transform.
                FlowPipe.Port flowInlet = this.transform.localPosition + inlet.LocalPosition;
                c.TryAddFlowObj( inlet, flowInlet );
            }

            return BuildFlowResult.Finished;
        }
        public virtual void ApplySnapshot( FlowNetworkSnapshot snapshot )
        {
            // apply the snapshot to the tank.
            if( snapshot.TryGetFlowObj( _cachedTank, out FlowTank tankSnapshot ) )
            {
                this.contents = tankSnapshot.Contents;
            }
        }


        void Start()
        {
            _cachedTank = new FlowTank();
            _cachedTank.SetNodes( triangulationPositions, inlets );
        }

        void FixedUpdate()
        {
            _cachedTank.DistributeFluids();
        }
    }
}