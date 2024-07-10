using KSS.Core.ReferenceFrames;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace KSS.Core.Components
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

        public static Quaternion GetSceneRotation( FControlFrame frame, Vessel fallback )
        {
            return frame == null
                ? fallback.ReferenceTransform.rotation
                : frame._referenceTransform.rotation;
        }

        public static QuaternionDbl GetAIRFRotation( FControlFrame frame, Vessel fallback )
        {
            return frame == null
                ? SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( fallback.ReferenceTransform.rotation )
                : SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( frame._referenceTransform.rotation );
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