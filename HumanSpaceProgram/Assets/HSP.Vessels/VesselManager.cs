using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vessels
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    public class VesselManager : SingletonMonoBehaviour<VesselManager>
    {
        private List<Vessel> _vessels = new List<Vessel>();

        /// <summary>
        /// Gets all vessels that are currently loaded into memory.
        /// </summary>
        public static IEnumerable<Vessel> LoadedVessels
        {
            get
            {
                return instance._vessels;
            }
        }

        public static int LoadedVesselCount
        {
            get
            {
                return instance._vessels.Count;
            }
        }

        internal static void Register( Vessel vessel )
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );

            instance._vessels.Add( vessel );
        }

        internal static void Unregister( Vessel vessel )
        {
            if( vessel == null )
                throw new ArgumentNullException( nameof( vessel ) );

            instance._vessels.Remove( vessel );
        }
    }
}