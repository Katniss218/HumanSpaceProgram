using HSP.Core;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.OverridableEvents;

namespace HSP.Vessels
{
    public class AfterVesselCreatedEvent : OverridableEvent<IVessel>
    {
        public static AfterVesselCreatedEvent Instance { get; } = new AfterVesselCreatedEvent();
    }
}

namespace HSP.Core
{
    public static class IVessel_Ex
    {
        public static bool IsRootOfVessel( this Transform part )
        {
            if( part.root != part.parent )
                return false;

            IVessel v = part.parent.GetComponent<IVessel>();
            if( v == null )
                return false;

            return v.RootPart == part;
        }

        /// <summary>
        /// Gets the <see cref="IVessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static IVessel GetVessel( this Transform part )
        {
            return part.root.GetComponent<IVessel>();
        }

        /// <summary>
        /// Gets the <see cref="IVessel"/> attached to this gameobject.
        /// </summary>
        /// <returns>The part object. Null if the gameobject is not part of a part object.</returns>
        public static IVessel GetVessel( this GameObject part )
        {
            return GetVessel( part.transform );
        }
    }

    /// <summary>
    /// Represents an arbitrary type of part object.
    /// </summary>
    public interface IVessel : IComponent
    {
        string DisplayName { get; set; }

        /// <summary>
        /// The root part of this part object (if any).
        /// </summary>
        Transform RootPart { get; set; }

        /// <summary>
        /// Returns the transform that defines the orientation (local space) of this part object.
        /// </summary>
        Transform ReferenceTransform { get; }

        IPhysicsObject PhysicsObject { get; set; }

        RootObjectTransform RootObjTransform { get; }

        /// <summary>
        /// Call this to rebuild the part data cache.
        /// </summary>
        void RecalculatePartCache();
    }
}