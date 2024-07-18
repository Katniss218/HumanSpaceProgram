using HSP.Vessels;
using HSP.Vessels.Serialization;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vessels.Components
{
    /// <summary>
    /// A marker component to track parts.
    /// </summary>
    public class FPart : MonoBehaviour
    {
        [field: SerializeField]
        public NamespacedIdentifier PartID { get; set; }

        public static PartMetadata GetPart( Transform obj )
        {
            while( obj != null )
            {
                FPart part = obj.GetComponent<FPart>();
                if( part != null )
                {
                    return PartRegistry.LoadMetadata( part.PartID );
                }
                obj = obj.parent;
            }
            return null;
        }

        [MapsInheritingFrom( typeof( FPart ) )]
        public static SerializationMapping FPartMapping()
        {
            return new MemberwiseSerializationMapping<FPart>()
            {
                ("part_id", new Member<FPart, NamespacedIdentifier>( o => o.PartID ))
            };
        }
    }
}