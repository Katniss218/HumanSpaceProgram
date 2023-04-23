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

        /// <summary>
        /// Creates a new partless vessel at the specified global position.
        /// </summary>
        /// <param name="airfPosition">The `Absolute Inertial Reference Frame` position of the vessel to create.</param>
        /// <param name="rotation">Rotation of the vessel in the `Absolute Inertial Reference Frame`</param>
        /// <returns>The created partless vessel.</returns>
        public Vessel CreatePartless( Vector3Dbl airfPosition, Quaternion rotation )
        {
            Vessel vessel = CreateGO( ReferenceFrames.SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( airfPosition ), rotation );

            vessel.SetPosition( airfPosition );

            return vessel;
        }

        private static Vessel CreateGO( Vector3 scenePosition, Quaternion sceneRotation )
        {
            GameObject vesselGO = new GameObject( $"Vessel, '{name}'" );
            vesselGO.transform.SetPositionAndRotation( scenePosition, sceneRotation );

            Vessel vessel = vesselGO.AddComponent<Vessel>();
            vessel.name = name;

            return vessel;
        }

        /// <summary>
        /// Completely deletes a vessel and cleans up after it.
        /// </summary>
        public static void Destroy( Vessel vessel )
        {
            UnityEngine.Object.Destroy( vessel.gameObject );
        }
    }
}
