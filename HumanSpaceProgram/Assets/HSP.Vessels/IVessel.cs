using HSP.ReferenceFrames;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vessels
{
    public interface IVessel
    {
        public IReadonlyVesselAttachmentGraph Attachments { get; }
        public IEnumerable<IReadonlyVesselIsland> Islands { get; }
        public IEnumerable<VesselPart> Parts { get; }

        /// <summary>
        /// Returns the transform that represents the local space of the vessel.
        /// </summary>
        public Transform ReferenceTransform { get; }
        public IPhysicsTransform PhysicsTransform { get; }
        public IReferenceFrameTransform ReferenceFrameTransform { get; }
    }
}
