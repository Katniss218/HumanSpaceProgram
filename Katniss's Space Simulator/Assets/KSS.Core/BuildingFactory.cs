using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A class responsible for instantiating a vessel from a source (save file, on launch, etc).
    /// </summary>
    public static class BuildingFactory
    {
        // add source (save file / in memory scene change, etc).

        const string name = "tempname_building";

        /// <summary>
        /// Creates a new partless vessel at the specified global position.
        /// </summary>
        /// <param name="airfPosition">The `Absolute Inertial Reference Frame` position of the vessel to create.</param>
        /// <param name="airfRotation">Rotation of the vessel in the `Absolute Inertial Reference Frame`</param>
        /// <returns>The created partless vessel.</returns>
        public static Building CreatePartless( CelestialBody referenceBody, Vector3Dbl localPosition, QuaternionDbl localRotation )
        {
            Building building = CreateGO( referenceBody, localPosition, localRotation );

            return building;
        }

        private static Building CreateGO( CelestialBody referenceBody, Vector3Dbl localPosition, QuaternionDbl localRotation )
        {
            GameObject gameObject = new GameObject( $"Building, '{name}'" );

            Building building = gameObject.AddComponent<Building>();
            building.name = name;
            building.ReferenceBody = referenceBody;
            building.ReferencePosition = localPosition;
            building.ReferenceRotation = localRotation;

            return building;
        }

        /// <summary>
        /// Completely deletes a vessel and cleans up after it.
        /// </summary>
        public static void Destroy( Building building )
        {
            UnityEngine.Object.Destroy( building.gameObject );
        }
    }
}