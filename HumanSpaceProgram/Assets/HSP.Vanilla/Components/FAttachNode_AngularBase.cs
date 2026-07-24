using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Components
{
    public class FAttachNode_AngularBase : FAttachNode
    {
        [field: SerializeField]
        public float MaxAngle { get; set; } = 180f;

        [field: SerializeField]
        public float MinAngle { get; set; } = -180f;
        
        [field: SerializeField]
        public float Spring { get; set; } = 0f;

        [field: SerializeField]
        public float Damping { get; set; } = 0f;
    }
}
