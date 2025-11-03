using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class FlowNode
    {
        public Vector3 pos;

#warning MAYBE - maybe mark as inlet here

        public FlowNode(Vector3 pos)
        {
            this.pos = pos;
        }
    }
}