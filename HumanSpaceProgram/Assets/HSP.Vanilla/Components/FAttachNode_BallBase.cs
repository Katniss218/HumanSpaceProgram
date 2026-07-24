using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Components
{
    public class FAttachNode_BallBase : FAttachNode
    {
        
        [field: SerializeField]
        public float XLimit { get; set; } = 45f;
        public float YLimit { get; set; } = 45f;
        // which one is twist? // add /// doccomments too.
        [field: SerializeField]
        public float ZLimit { get; set; } = 45f;

        [field: SerializeField]
        public float Spring { get; set; } = 0f;

        [field: SerializeField]
        public float Damping { get; set; } = 0f;
    }
}
