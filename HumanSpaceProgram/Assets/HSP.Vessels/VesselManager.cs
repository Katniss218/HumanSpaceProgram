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
                // if( !instanceExists )
                //     throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

                return instance._vessels;
            }
        }

        public static int LoadedVesselCount
        {
            get
            {
                // if( !instanceExists )
                //     throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

                return instance._vessels.Count;
            }
        }

        internal static void Register( Vessel vessel )
        {
            // if( !instanceExists )
            //     throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

            instance._vessels.Add( vessel );
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_VESSEL_REGISTERED, vessel );
        }

        internal static void Unregister( Vessel vessel )
        {
            // if( !instanceExists )
            //     throw new InvalidSceneManagerException( $"{nameof( VesselManager )} is only available in the gameplay scene." );

            instance._vessels.Remove( vessel );
            HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_VESSEL_UNREGISTERED, vessel );
        }
    }
}