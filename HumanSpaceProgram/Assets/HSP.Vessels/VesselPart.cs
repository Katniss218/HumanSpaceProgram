using HSP.Content;
using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vessels
{
    /// <summary>
    /// A marker component to track parts.
    /// </summary>
    public class VesselPart : MonoBehaviour
    {
#warning TODO - something needs to set the vessel.
        /// <summary>
        /// Gets or sets the vessel that this part belongs to. 
        /// </summary>
        public Vessel Vessel { get; internal set; }

        /// <summary>
        /// The ID of this part type (not instance).
        /// </summary>
        public NamespacedID PartID { get; set; }

        public static PartMetadata GetPart( Transform obj )
        {
            while( obj != null )
            {
                if( obj.HasComponent( out VesselPart part ) )
                {
                    if( PartRegistry.TryLoadMetadata( part.PartID, out var metadata ) )
                        return metadata;

                    Debug.LogError( $"Failed to load part metadata for FPart with part ID '{part.PartID}'." );
                    return null;
                }
                obj = obj.parent;
            }
            return null;
        }

        [MapsInheritingFrom( typeof( VesselPart ) )]
        public static IDescriptor VesselPartMapping()
        {
            return new MemberwiseDescriptor<VesselPart>()
                .WithMember( "part_id", o => o.PartID );
        }
    }
}