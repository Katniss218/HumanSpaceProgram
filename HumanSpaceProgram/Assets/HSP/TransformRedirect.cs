using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP
{
    /// <summary>
    /// Redirects events about an object to a different object.
    /// </summary>
    public class TransformRedirect : MonoBehaviour
    {
        /// <summary>
        /// The click action on the object of this component will be redirected to this target.
        /// </summary>
        [field: SerializeField]
        public Transform Target { get; set; }

        /// <summary>
        /// If the transform has an <see cref="TransformRedirect"/> component that has a target - returns its target, otherwise returns the input transform.
        /// </summary>
        public static Transform TryRedirect( Transform transform )
        {
            if( transform.HasComponent<TransformRedirect>( out var redirect ) && redirect.Target != null )
            {
                return redirect.Target;
            }
            return transform;
        }


        [MapsInheritingFrom( typeof( TransformRedirect ) )]
        public static SerializationMapping FClickInteractionRedirectMapping()
        {
            return new MemberwiseSerializationMapping<TransformRedirect>()
            {
                ("target", new Member<TransformRedirect, Transform>( ObjectContext.Ref, o => o.Target ))
            };
        }
    }
}