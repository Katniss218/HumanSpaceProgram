using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class FlowTetrahedron
    {
        public readonly FlowNode v0;
        public readonly FlowNode v1;
        public readonly FlowNode v2;
        public readonly FlowNode v3;

        public FlowTetrahedron( FlowNode v0, FlowNode v1, FlowNode v2, FlowNode v3 )
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public static float GetVolume( Vector3 v0Pos, Vector3 v1Pos, Vector3 v2Pos, Vector3 v3Pos )
        {
            float parallelepipedVolume = Vector3.Dot( v1Pos - v0Pos, Vector3.Cross( v2Pos - v0Pos, v3Pos - v0Pos ) );
            return Mathf.Abs( parallelepipedVolume ) / 6f;
        }

        public float GetVolume()
        {
            return GetVolume( v0.pos, v1.pos, v2.pos, v3.pos );
        }
    }
}