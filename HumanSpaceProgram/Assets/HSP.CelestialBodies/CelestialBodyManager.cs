using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.CelestialBodies
{
    public class CelestialBodyManager : SingletonMonoBehaviour<CelestialBodyManager>
    {
        private Dictionary<string, CelestialBody> _celestialBodies = new Dictionary<string, CelestialBody>();

        public static CelestialBody Get( string id )
        {
#warning TODO - ManagerNotFoundException? or SingletonMonoBehaviourNotFoundException?
           // if( !instanceExists )
           //     throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            if( instance._celestialBodies.TryGetValue( id, out CelestialBody body ) )
            {
                return body;
            }

            return null;
        }

        /// <summary>
        /// Gets all celestial bodies that are currently loaded into memory.
        /// </summary>
        public static IEnumerable<CelestialBody> CelestialBodies
        {
            get
            {
               // if( !instanceExists )
               //     throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

                return instance._celestialBodies.Values;
            }
        }

        public static int CelestialBodyCount
        {
            get
            {
               // if( !instanceExists )
               //     throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

                return instance._celestialBodies.Count;
            }
        }

        internal static void Register( CelestialBody celestialBody )
        {
           // if( !instanceExists )
           //     throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            instance._celestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
        {
           // if( !instanceExists )
           //     throw new InvalidSceneManagerException( $"{nameof( CelestialBodyManager )} is only available in the gameplay scene." );

            instance._celestialBodies.Remove( id );
        }


        private static GameObject[] GetAllRootGameObjects()
        {
            return instance._celestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }
    }
}