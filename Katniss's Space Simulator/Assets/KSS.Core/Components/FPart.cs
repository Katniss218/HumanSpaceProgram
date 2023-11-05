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
        public string PartID { get; set; }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "part_id", this.PartID }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "part_id", out var partId ) )
                this.PartID = (string)partId;
        }

        public static PartMetadata GetPart( Transform obj )
        {
            while( obj != null )
            {
                FPart part = obj.GetComponent<FPart>();
                if( part != null )
                {
                    return PartHelper.GetPartMetadata( part.PartID );
                }
                obj = obj.parent;
            }
            return null;
        }
    }
}