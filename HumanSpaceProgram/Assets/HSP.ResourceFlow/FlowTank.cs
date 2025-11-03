using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HSP.ResourceFlow
{
    public class FResourceContainer_FlowTank : IResourceContainer, IResourceProducer, IResourceConsumer
    {
        Vector3 _triangulationPositions; // initial pos for triangulation.

        FlowTank _tank;
    }

    public sealed class FlowInlet
    {
        public float nominalArea; // m^2
        public FlowNode node;
    }

    public sealed class FlowTank
    {
        private FlowTetrahedron[] _tetrahedra;
        private FlowNode[] _nodes;
        private FlowEdge[] _edges;
        private SubstanceStateCollection[] _substancesInEdges;

        private Dictionary<FlowNode, FlowInlet> _inletNodes; // inlets and outlets (ports/holes in the tank). If nothing is attached, the inlet is treated as a hole.
#warning TODO - mixing.
        private SubstanceStateCollection _inflow;
        private SubstanceStateCollection _outflow;

#warning TODO - store the original nodes?

#warning TODO - store which nodes were moved from their original positions and by what delta.

        private double _calculatedVolume; // volume calculated from tetrahedra
        private double _volume;

        private static void DistributeVolumes( FlowTetrahedron[] tetrahedra, FlowEdge[] edges )
        {
            // distribute tetrahedra volumes to edges.
        }

        private void AddNodes( FlowNode[] nodes )
        {
            // todo - add nodes and triangulate them. redistribute volume and settle already existing fluid.
        }

        private FlowInlet CreateInlet( Vector3 localPos, float area )
        {

        }

        private void RemoveInlet( FlowInlet node )
        {

        }
    }
}