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
    public class FPart : MonoBehaviour, IPersistsData
    {
        [field: SerializeField]
        public NamespacedIdentifier PartID { get; set; }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)Persistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "part_id", s.WriteNamespacedIdentifier( this.PartID ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            Persistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "part_id", out var partId ) )
                this.PartID = l.ReadNamespacedIdentifier( partId );
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