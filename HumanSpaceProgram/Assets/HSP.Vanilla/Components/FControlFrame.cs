using UnityEngine;
using UnityPlus.Serialization;
using Ctx = UnityPlus.Serialization.Ctx;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Represents a coordinate system that can be used as the control frame for avionics.
    /// </summary>
    public sealed class FControlFrame : MonoBehaviour
    {
        /// <summary>
        /// The transform to use as the coordinate system.
        /// </summary>
        [SerializeField]
        private Transform _referenceTransform;

        public Quaternion GetRotation()
        {
            return this._referenceTransform.rotation;
        }

        [MapsInheritingFrom( typeof( FControlFrame ) )]
        public static IDescriptor FControlFrameMapping()
        {
            return new MemberwiseDescriptor<FControlFrame>()
                .WithMember( "reference_transform", typeof( Ctx.Ref ), o => o._referenceTransform );
        }
    }
}