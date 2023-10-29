using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core.DesignScene
{
    public class DesignVesselManager : SingletonMonoBehaviour<DesignVesselManager>
    {
        static JsonExplicitHierarchyGameObjectsStrategy _vesselStrategy = new JsonExplicitHierarchyGameObjectsStrategy( GetGameObjects );

        /// <summary>
        /// Modify this to point at a different craft file.
        /// </summary>
        public static VesselMetadata Metadata { get; set; }

        private static IPartObject _vessel;

        // undos stored in files, preserved across sessions?

        public static void SaveVessel()
        {
            // save current vessel to the files defined by metadata's ID.
        }

        public static void LoadVessel()
        {
            // load current vessel from the files defined by metadata's ID.
        }

        // ------

        private static GameObject[] GetGameObjects()
        {
            return new GameObject[] { _vessel.gameObject };
        }
    }
}