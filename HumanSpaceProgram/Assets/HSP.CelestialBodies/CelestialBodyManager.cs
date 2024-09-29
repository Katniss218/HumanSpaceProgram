using System;
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
                return instance._celestialBodies.Values;
            }
        }

        public static int CelestialBodyCount
        {
            get
            {
                return instance._celestialBodies.Count;
            }
        }

        internal static void Register( CelestialBody celestialBody )
        {
            if( celestialBody == null )
                throw new ArgumentNullException( nameof( celestialBody ) );
            if( celestialBody.ID == null )
                throw new ArgumentException( $"Can't register a celestial body that has a null ID.", nameof( celestialBody ) );

            instance._celestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
        {
            if( id == null )
                throw new ArgumentException( $"Can't unregister a celestial body that has a null ID.", nameof( id ) );

            instance._celestialBodies.Remove( id );
        }


        private static GameObject[] GetAllRootGameObjects()
        {
            return instance._celestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }
    }
}