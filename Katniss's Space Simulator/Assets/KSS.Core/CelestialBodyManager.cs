using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public class CelestialBodyManager : SerializedManager, IPersistent
    {
        public static Dictionary<Guid, CelestialBody> CelestialBodies { get; private set; }

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
            return CelestialBodies.Values.Select( cb => cb.gameObject ).ToArray();
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            // nothing yet.
        }
    }
}