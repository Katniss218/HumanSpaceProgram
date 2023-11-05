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
    /// Redirects click interactions to a different <see cref="GameObject"/>.
    /// </summary>
    [RequireComponent( typeof( Collider ) )]
    public class FClickInteractionRedirect : MonoBehaviour, IPersistent
    {
        /// <summary>
        /// The click action on the object of this component will be redirected to this target.
        /// </summary>
        [field: SerializeField]
        public GameObject Target { get; set; }

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
                this.Target = (GameObject)l.ReadObjectReference( target );
        }
    }
}