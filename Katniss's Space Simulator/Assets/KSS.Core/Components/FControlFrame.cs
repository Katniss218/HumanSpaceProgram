using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core.Components
{
    /// <summary>
    /// Represents a coordinate system that can be used as the control frame for avionics.
    /// </summary>
    public sealed class FControlFrame : MonoBehaviour
    {
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

        [SerializationMappingProvider( typeof( FControlFrame ) )]
        public static SerializationMapping FControlFrameMapping()
        {
            return new MemberwiseSerializationMapping<FControlFrame>()
            {
                ("reference_transform", new Member<FControlFrame, Transform>( ObjectContext.Ref, o => o._referenceTransform ))
            }
            .IncludeMembers<Behaviour>()
            .UseBaseTypeFactory();
        }
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "reference_transform", s.WriteObjectReference( _referenceTransform ) }
            };
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "reference_transform", out var referenceTransform ) )
                _referenceTransform = (Transform)l.ReadObjectReference( referenceTransform );
        }*/
    }
}