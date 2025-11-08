using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Holds fluid inside a <see cref="FlowTank"/>.
    /// </summary>
    public readonly struct FlowEdge
    {
        public readonly FlowNode end1;
        public readonly FlowNode end2;

        public float Volume { get; }

        public FlowEdge( FlowNode end1, FlowNode end2, float volume )
        {
            this.end1 = end1;
            this.end2 = end2;
            this.Volume = volume;
        }

        public float GetLength()
        {
            return Vector3.Distance( end1.pos, end2.pos );
        }

        public Vector3 GetCenter()
        {
            return (end1.pos + end2.pos) * 0.5f;
        }

        public float GetProjectedHeight( Vector3 accelerationDir )
        {
            Vector3 edgeDir = (end2.pos - end1.pos).normalized;
            return Mathf.Abs( Vector3.Dot( edgeDir, accelerationDir.normalized ) ) * GetLength();
        }
    }
}