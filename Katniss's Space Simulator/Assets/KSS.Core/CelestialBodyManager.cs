using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public class CelestialBodyManager : MonoBehaviour
    {
        public static List<CelestialBody> CelestialBodies { get; set; }

        void Awake()
        {
            CelestialBodies = new List<CelestialBody>();
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