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
    public sealed class VesselFactory
    {
        // add source (save file / in memory scene change, etc).

        const string name = "tempname_vessel";


#warning TODO - We need to shift the reference frame before spawning the active vessel. Same for switching vessels, or spawning anything that needs precision really
        // Also need to store a precise position for anything that is expected to be moved away far enough and then brought back without being unloaded and reloaded
        // - (precision issues will round the positions of its constituent parts to the nearest available slot).
        
        public Vessel CreatePartless( Vector3 position, Quaternion rotation )
        {
            Vessel vessel = CreateGO( position, rotation );

            return vessel;
        }

        public Vessel Create( Vector3 position, Quaternion rotation, Func<Vessel, Part> rootPart )
        {
            Vessel vessel = CreateGO( position, rotation );

            rootPart( vessel );

            // A vessel is the only physical "part" of the rocket. Parts are not physical.
            // Add parts and more.

            return vessel;
        }

        private Vessel CreateGO( Vector3 position, Quaternion rotation )
        {
            GameObject vesselGO = new GameObject( $"Vessel, '{name}'" );
            vesselGO.transform.SetPositionAndRotation( position, rotation );

            Vessel vessel = vesselGO.AddComponent<Vessel>();
            vessel.name = name;

            return vessel;
        }

        public static void Destroy( Vessel vessel )
        {
            UnityEngine.Object.Destroy( vessel.gameObject );
            // deletes the vessel and cleans all references to it.
        }
    }
}
