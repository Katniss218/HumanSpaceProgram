using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    public class VesselManager : MonoBehaviour
    {
        public static Vessel ActiveVessel { get; set; }

        public static List<Vessel> Vessels { get; set; }

        void Awake()
        {
            Vessels = new List<Vessel>();
        }

        internal static GameObject[] GetAllRootGameObjects()
        {

            GameObject[] gos = new GameObject[Vessels.Count];
            for( int i = 0; i < Vessels.Count; i++ )
            {
                gos[i] = Vessels[i].gameObject;
            }
            return gos;
        }

        // save/load
    }
}