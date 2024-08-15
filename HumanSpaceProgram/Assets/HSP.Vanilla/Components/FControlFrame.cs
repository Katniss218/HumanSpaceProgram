using HSP.ReferenceFrames;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Represents a coordinate system that can be used as the control frame for avionics.
    /// </summary>
    public sealed class FControlFrame : MonoBehaviour
    {
#warning TODO - save this (and in general handle this properly, with a selector in the UI where the player can click "control from here").
#warning TODO - handle this appropriately (and per-control system)
        public static FControlFrame VesselControlFrame { get; set; }

        /// <summary>
        /// The transform to use as the coordinate system.
        /// </summary>
        [SerializeField]
        private Transform _referenceTransform;

        /// <summary>
        /// Tries to get the rotation of the control frame (in scene space). Falls back to the rotation of the vessel if unavailable.
        /// </summary>
        /// <returns>The rotation of the specified control frame, or the vessel.</returns>
        public static Quaternion GetRotation( FControlFrame frame, Vessel fallback )
        {
            return (frame != null && frame._referenceTransform != null)
                ? frame._referenceTransform.rotation
                : fallback.ReferenceTransform.rotation;
        }

        /// <summary>
        /// Tries to get the rotation of the control frame (in absolute inertial space). Falls back to the rotation of the vessel if unavailable.
        /// </summary>
        /// <returns>The rotation of the specified control frame, or the vessel.</returns>
        public static QuaternionDbl GetAbsoluteRotation( FControlFrame frame, Vessel fallback )
        {
            return (frame != null && frame._referenceTransform != null)
                ? SceneReferenceFrameManager.ReferenceFrame.TransformRotation( frame._referenceTransform.rotation )
                : SceneReferenceFrameManager.ReferenceFrame.TransformRotation( fallback.ReferenceTransform.rotation );
        }

        [MapsInheritingFrom( typeof( FControlFrame ) )]
        public static SerializationMapping FControlFrameMapping()
        {
            return new MemberwiseSerializationMapping<FControlFrame>()
            {
                ("reference_transform", new Member<FControlFrame, Transform>( ObjectContext.Ref, o => o._referenceTransform ))
            };
        }
    }
}