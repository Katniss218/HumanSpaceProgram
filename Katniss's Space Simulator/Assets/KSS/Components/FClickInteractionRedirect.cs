using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    [RequireComponent( typeof( Collider ) )]
    public class FClickInteractionRedirect : MonoBehaviour, IPersistent
    {
        [field: SerializeField]
        public Transform Target { get; set; }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "target", s.WriteObjectReference( this.Target ) }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "target", out var target ) )
                this.Target = (Transform)l.ReadObjectReference( target );
        }
    }
}