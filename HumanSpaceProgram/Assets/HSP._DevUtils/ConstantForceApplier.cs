using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP._DevUtils
{
    public class ConstantForceApplier : MonoBehaviour
    {
        public IPhysicsTransform Vessel { get; set; }

        public Vector3 Force { get; set; }

        void FixedUpdate()
        {
            Vessel.AddForce( Force );
        }
    }
}