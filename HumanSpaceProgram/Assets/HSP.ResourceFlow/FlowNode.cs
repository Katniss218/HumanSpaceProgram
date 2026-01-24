using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class FlowNode
    {
        public readonly Vector3 pos;

        public FlowNode(Vector3 pos)
        {
            this.pos = pos;
        }
    }
}