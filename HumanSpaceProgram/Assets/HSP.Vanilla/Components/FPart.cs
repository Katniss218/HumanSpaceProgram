using HSP.Content;
using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vessels.Components
{
    /// <summary>
    /// A marker component to track parts.
    /// </summary>
    public class FPart : MonoBehaviour
    {
        [field: SerializeField]
        public NamespacedID PartID { get; set; }

        public static PartMetadata GetPart( Transform obj )
        {
            while( obj != null )
            {
                if( obj.HasComponent( out FPart part ) )
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

        [MapsInheritingFrom( typeof( FPart ) )]
        public static IDescriptor FPartMapping()
        {
            return new MemberwiseDescriptor<FPart>()
                .WithMember( "part_id", o => o.PartID );
        }
    }
}