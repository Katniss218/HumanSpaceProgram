using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Components
{
    public class FAttachNode_LinearBase : FAttachNode
    {
        [field: SerializeField]
        public float MaxLimit { get; set; } = 2.0f;

        [field: SerializeField]
        public float MinLimit { get; set; } = 0.0f;
        
        [field: SerializeField]
        public float Spring { get; set; } = 0f;

        [field: SerializeField]
        public float Damping { get; set; } = 0f;
    }
}
