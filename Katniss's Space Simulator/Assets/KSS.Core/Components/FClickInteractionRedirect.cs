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


        /// <summary>
        /// If the object has an <see cref="FClickInteractionRedirect"/> component that has a target - returns its target, otherwise returns the input object.
        /// </summary>
        public static GameObject TryRedirect( GameObject gameObject )
        {
            if( gameObject.HasComponent<FClickInteractionRedirect>( out var redirect ) && redirect.Target != null )
            {
                return redirect.Target;
            }
            return gameObject;
        }

        /// <summary>
        /// If the transform has an <see cref="FClickInteractionRedirect"/> component that has a target - returns its target, otherwise returns the input transform.
        /// </summary>
        public static Transform TryRedirect( Transform transform )
        {
            if( transform.HasComponent<FClickInteractionRedirect>( out var redirect ) && redirect.Target != null )
            {
                return redirect.Target.transform;
            }
            return transform;
        }


        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "target", s.WriteObjectReference( this.Target ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "target", out var target ) )
                this.Target = (GameObject)l.ReadObjectReference( target );
        }
    }
}