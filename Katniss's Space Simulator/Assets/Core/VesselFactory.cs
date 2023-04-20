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
            Vessel vessel = CreateGO();

            rootPart( vessel );

            // A vessel is the only physical "part" of the rocket. Parts are not physical.
            // Add parts and more.

            return vessel;
        }

# warning TODO - this is kinda ugly. Allow empty vessels maybe? And then assign parts to that vessel. Ideally specifying a part to attach to as well.
        public Vessel Create( Part existingVesselPart )
        {
            Vessel vessel = CreateGO();

            existingVesselPart.SetVesselHierarchy( vessel );
            existingVesselPart.SetParent( null );
            vessel.SetRootPart( existingVesselPart );

            return vessel;
        }

        private Vessel CreateGO()
        {
            GameObject vesselGO = new GameObject( $"Vessel, '{name}'" );

            Vessel vessel = vesselGO.AddComponent<Vessel>();
            vessel.name = name;

            return vessel;
        }

        public static void Destroy( Vessel vessel )
        {
            UnityEngine.Object.Destroy( vessel );
            // deletes the vessel and cleans all references to it.
        }
    }
}
