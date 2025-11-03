using System.Collections.Generic;

namespace HSP.ResourceFlow
{
    public class FlowNetwork
    {
        public readonly List<FlowTank> Tanks = new List<FlowTank>();
        public readonly List<FlowPipe> Pipes = new List<FlowPipe>();

        public void Solve( float dt )
        {
            // compute flows
            var flows = new List<(FlowPipe pipe, float vol)>();
            foreach( var p in Pipes )
            {
                var q = p.ComputeFlowRate(); // m^3/s
                var vol = q * dt; // m^3
                flows.Add( (p, vol) );
            }

            // TODO solve for the new pressures and fluid movement using the flowrates.
        }
    }
}