using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vessels
{
    public interface IVessel
    {
        public Transform RootPart { get; }
        public Transform ReferenceTransform { get; }
        public IPhysicsTransform PhysicsTransform { get; }
        public IReferenceFrameTransform ReferenceFrameTransform { get; }
    }
}
