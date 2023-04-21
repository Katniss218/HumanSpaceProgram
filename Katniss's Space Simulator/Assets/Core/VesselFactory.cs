using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// A class responsible for instantiating a vessel from a source (save file, on launch, etc).
    /// </summary>
    public class VesselFactory
    {
        // add source (save file / in memory scene change, etc).

        const string name = "tempname_vessel";

        public Vessel CreatePartless()
        {
            Vessel vessel = CreateGO();

            return vessel;
        }

        public Vessel Create( Func<Vessel, Part> rootPart )
        {
#warning TODO - position and reference frame?
            Vessel vessel = CreateGO();

            rootPart( vessel );

            // A vessel is the only physical "part" of the rocket. Parts are not physical.
            // Add parts and more.

            return vessel;
        }

        private Vessel CreateGO()
        {
            GameObject vesselGO = new GameObject( $"Vessel, '{name}'" );

            Vessel vessel = vesselGO.AddComponent<Vessel>();
            vessel.name = name;

            StandaloneMoveable sm = vesselGO.AddComponent<StandaloneMoveable>();

            return vessel;
        }

        public static void Destroy( Vessel vessel )
        {
            UnityEngine.Object.Destroy( vessel.gameObject );
            // deletes the vessel and cleans all references to it.
        }
    }
}
