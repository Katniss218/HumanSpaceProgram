using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vessels
{
    public interface IVessel
    {
        public Transform RootPart { get; }
        /// <summary>
        /// Returns the transform that represents the local space of the vessel.
        /// </summary>
        public Transform ReferenceTransform { get; }
        public IPhysicsTransform PhysicsTransform { get; }
        public IReferenceFrameTransform ReferenceFrameTransform { get; }
    }
}
