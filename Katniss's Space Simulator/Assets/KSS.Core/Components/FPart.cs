using KSS.Core.Mods;
using KSS.Core.Serialization;
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

        [SerializationMappingProvider( typeof( FPart ) )]
        public static SerializationMapping FPartMapping()
        {
            return new MemberwiseSerializationMapping<FPart>()
            {
                ("part_id", new Member<FPart, NamespacedIdentifier>( o => o.PartID ))
                // todo - conditions.
            }
            .UseBaseTypeFactory()
            .IncludeMembers<Behaviour>();
        }
    }
}