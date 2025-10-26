using HSP.CelestialBodies;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <remarks>
    /// A physics transform that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
    /// </remarks>
	[RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class PinnedCelestialBodyReferenceFrameTransform : PinnedReferenceFrameTransform
    {
        ICelestialBody _referenceBody = null;

        public ICelestialBody ReferenceBody
        {
            get => _referenceBody;
            set
            {
                _referenceBody = value;
                base.ReferenceTransform = value?.ReferenceFrameTransform;
            }
        }

        public void SetReference( ICelestialBody referenceBody, Vector3Dbl referencePosition, QuaternionDbl referenceRotation )
        {
            _referenceBody = referenceBody;
            base.SetReference( referenceBody?.ReferenceFrameTransform, referencePosition, referenceRotation );
        }


        [MapsInheritingFrom( typeof( PinnedCelestialBodyReferenceFrameTransform ) )]
        public static SerializationMapping PinnedCelestialBodyReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<PinnedCelestialBodyReferenceFrameTransform>()
                .ExcludeMember( "reference_transform" ) // removes the base member because we use the celestialbody instead.
                .WithMember( "reference_body", o => o.ReferenceBody == null ? null : o.ReferenceBody.ID, ( o, value ) => o.ReferenceBody = value == null ? null : CelestialBodyManager.Get( value ) );
        }
    }
}