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
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "part_id", this.PartID.GetData() }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "part_id", out var partId ) )
                this.PartID = partId.AsNamespacedIdentifier();
        }
        */
        [SerializationMappingProvider( typeof( FPart ) )]
        public static SerializationMapping FConstructibleMapping()
        {
            return new CompoundSerializationMapping<FPart>()
            {
                ("part_id", new Member<FPart, NamespacedIdentifier>( o => o.PartID ))
                // todo - conditions.
            }
            .UseBaseTypeFactory()
            .IncludeMembers<Behaviour>();
        }

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
    }
}