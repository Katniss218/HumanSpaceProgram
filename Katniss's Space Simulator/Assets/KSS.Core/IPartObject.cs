using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public static class IPartObjectEx
    {
        public static bool IsRootOfPartObject( this Transform part )
        {
            if( part.root != part.parent )
                return false;

            IPartObject v = part.parent.GetComponent<IPartObject>();
            if( v == null )
                return false;

            return v.RootPart == part;
        }

        /// <summary>
        /// Gets the <see cref="IPartObject"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static IPartObject GetPartObject( this Transform part )
        {
            return part.root.GetComponent<IPartObject>();
        }
    }

    public interface IPartObject : IComponent
    {
        Transform RootPart { get; }
        IPhysicsObject PhysicsObject { get; }
        RootObjectTransform RootObjTransform { get; }
    }
}