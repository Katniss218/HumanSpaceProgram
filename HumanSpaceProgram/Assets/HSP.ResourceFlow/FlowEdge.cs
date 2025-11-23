using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Holds fluid inside a <see cref="FlowTank2"/>.
    /// </summary>
    public readonly struct FlowEdge
    {
        public readonly int end1;
        public readonly int end2;

        public double Volume { get; }

        public FlowEdge( int end1, int end2, double volume )
        {
            this.end1 = end1;
            this.end2 = end2;
            this.Volume = volume;
        }
    }
}