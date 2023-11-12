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
    /// A marker component to track predefined parts.
    /// </summary>
    public class FPart : MonoBehaviour, IPersistent
    {
        [field: SerializeField]
        public NamespacedID PartID { get; set; }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
#warning TODO - extension to serialize directly.
                { "part_id", this.PartID.ToString() }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "part_id", out var partId ) )
                this.PartID = NamespacedID.Parse( (string)partId );
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