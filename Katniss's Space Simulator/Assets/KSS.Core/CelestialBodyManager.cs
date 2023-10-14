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
        public static Dictionary<string, CelestialBody> CelestialBodies { get; private set; }

        public static CelestialBody Get( string id )
        {
            if( CelestialBodies != null
             && CelestialBodies.TryGetValue( id, out CelestialBody body ) )
            {
                return body;
            }

            return null;
        }

        internal static void Register( CelestialBody celestialBody )
        {
            if( CelestialBodies == null )
                CelestialBodies = new Dictionary<string, CelestialBody>();

            CelestialBodies[celestialBody.ID] = celestialBody;
        }

        internal static void Unregister( string id )
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