using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public class CelestialBodyManager : SerializedManager
    {
        public static Dictionary<Guid, CelestialBody> CelestialBodies { get; set; }

        /// <summary>
        /// Registers a celestial body instance under the specified ID.
        /// </summary>
        public static void RegisterCelestialBody( Guid id, CelestialBody celestialBody )
        {
            if( CelestialBodies == null )
                CelestialBodies = new Dictionary<Guid, CelestialBody>();

            CelestialBodies.Add( id, celestialBody );
        }

        /// <summary>
        /// Unregisters a celestial body instance with the specified ID.
        /// </summary>
        public static void UnregisterCelestialBody( Guid id )
        {
            if( CelestialBodies != null )
                CelestialBodies.Remove( id );
        }

        internal static GameObject[] GetAllRootGameObjects()
        {
            GameObject[] gos = new GameObject[CelestialBodies.Count];
            for( int i = 0; i < CelestialBodies.Count; i++ )
            {
                gos[i] = CelestialBodies[i].gameObject;
            }
            return gos;
        }

        // save/load
    }
}