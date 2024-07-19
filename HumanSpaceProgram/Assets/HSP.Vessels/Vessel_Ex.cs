using UnityEngine;

namespace HSP.Vessels
{
    public static class Vessel_Ex
    {
        public static bool IsRootOfVessel( this Transform part )
        {
            if( part.root != part.parent )
                return false;

            Vessel v = part.parent.GetComponent<Vessel>();
            if( v == null )
                return false;

            return v.RootPart == part;
        }

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static Vessel GetVessel( this Transform part )
        {
            return part.root.GetComponent<Vessel>();
        }

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static bool HasVessel( this Transform part )
        {
            return part.root.GetComponent<Vessel>() != null;
        }

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static bool HasVessel( this Transform part, out Vessel vessel )
        {
            vessel = part.root.GetComponent<Vessel>();
            return vessel != null;
        }
    }
}