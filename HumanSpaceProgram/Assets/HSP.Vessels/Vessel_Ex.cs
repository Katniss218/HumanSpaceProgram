using UnityEngine;

namespace HSP.Vessels
{
    public static class Vessel_Ex
    {
        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static T GetVessel<T>( this Transform part ) where T : IVessel
        {
            return part.root.GetComponent<T>();
        }

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static bool HasVessel<T>( this Transform part ) where T : IVessel
        {
            return part.root.GetComponent<T>() != null;
        }

        /// <summary>
        /// Gets the <see cref="Vessel"/> attached to this transform.
        /// </summary>
        /// <returns>The part object. Null if the transform is not part of a part object.</returns>
        public static bool HasVessel<T>( this Transform part, out T vessel ) where T : IVessel
        {
            vessel = part.root.GetComponent<T>();
            return vessel != null;
        }
    }
}