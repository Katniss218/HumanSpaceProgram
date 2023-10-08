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
        public static List<CelestialBody> CelestialBodies { get; set; }

        public static void RegisterCelestialBody( CelestialBody celestialBody )
        {
            if( CelestialBodies == null )
                CelestialBodies = new List<CelestialBody>();

            CelestialBodies.Add( celestialBody );
        }

        public static void UnregisterCelestialBody( CelestialBody celestialBody )
        {
            if( CelestialBodies != null )
                CelestialBodies.Remove( celestialBody );
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