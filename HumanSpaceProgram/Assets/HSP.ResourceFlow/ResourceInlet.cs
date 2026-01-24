using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ResourceFlow
{
    public sealed class ResourceInlet
    {
        /// <summary>
        /// The nominal cross-sectional area of the inlet, in [m^2].
        /// </summary>
        public float NominalArea;

        /// <summary>
        /// The local position of the inlet in the parent object's space.
        /// </summary>
        public Vector3 LocalPosition;

        public ResourceInlet()
        {
        }

        public ResourceInlet( float nominalArea, Vector3 localPosition )
        {
            NominalArea = nominalArea;
            LocalPosition = localPosition;
        }

        [MapsInheritingFrom( typeof( ResourceInlet ) )]
        public static SerializationMapping ResourceInletMapping()
        {
            return new MemberwiseSerializationMapping<ResourceInlet>()
                .WithMember( "nominal_area", o => o.NominalArea )
                .WithMember( "local_position", o => o.LocalPosition );
        }
    }
}