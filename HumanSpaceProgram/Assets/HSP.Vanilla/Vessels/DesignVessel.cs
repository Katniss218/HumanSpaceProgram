using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla.Vessels
{
    public class DesignVessel : PartGraph, IVessel
    {
        // Design-specific metadata and calculations go here
        
        /// <summary>
        /// Returns the transform that represents the local space of the vessel.
        /// </summary>
        public Transform ReferenceTransform => this.transform;
        
        public HSP.ReferenceFrames.IPhysicsTransform PhysicsTransform => null;
        public HSP.ReferenceFrames.IReferenceFrameTransform ReferenceFrameTransform => null;
    }
}
