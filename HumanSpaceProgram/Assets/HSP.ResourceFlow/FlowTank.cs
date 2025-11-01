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

    public sealed class FlowTank
    {
        private FlowTetrahedron[] _tetrahedra;
        private FlowNode[] _nodes;
        private FlowEdge[] _edges;

#warning TODO - store which nodes were moved from their original positions and by what delta.

        private double _calculatedVolume; // volume calculated from tetrahedra
        private double _volume;
    }
}