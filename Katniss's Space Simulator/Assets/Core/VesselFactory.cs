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

        public Vessel Create( Func<Vessel, Part> rootPart )
        {
            GameObject vesselGO = new GameObject( $"Vessel, '{name}'" );

            Vessel vessel = vesselGO.AddComponent<Vessel>();
            vessel.name = name;

            rootPart( vessel );

            // A vessel is the only physical "part" of the rocket. Parts are not physical.
            // Add parts and more.

            return vessel;
        }

        public Vessel Create( Part existingVesselPart )
        {

        }

        public static void Destroy( Vessel vessel )
        {
            // deletes the vessel and cleans all references to it.
        }
    }
}
