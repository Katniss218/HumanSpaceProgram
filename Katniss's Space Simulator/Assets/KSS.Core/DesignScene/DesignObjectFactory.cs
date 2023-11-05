using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.DesignScene
{
    /// <summary>
    /// A class responsible for instantiating a vessel from a source (save file, on launch, etc).
    /// </summary>
    public static class DesignObjectFactory
    {
        const string name = "design_object";

        /// <summary>
        /// Creates a new partless vessel at the specified global position.
        /// </summary>
        /// <param name="airfPosition">The `Absolute Inertial Reference Frame` position of the vessel to create.</param>
        /// <param name="airfRotation">Rotation of the vessel in the `Absolute Inertial Reference Frame`</param>
        /// <returns>The created partless vessel.</returns>
        public static DesignObject CreatePartless( Vector3 position, Quaternion rotation )
        {
            DesignObject vessel = CreateGO( position, rotation );

            return vessel;
        }

        private static DesignObject CreateGO( Vector3 position, Quaternion rotation )
        {
            GameObject gameObject = new GameObject( $"Vessel, '{name}'" );

            DesignObject vessel = gameObject.AddComponent<DesignObject>();
            vessel.name = name;
            vessel.transform.SetPositionAndRotation( position, rotation );

            return vessel;
        }

        /// <summary>
        /// Completely deletes a vessel and cleans up after it.
        /// </summary>
        public static void Destroy( DesignObject vessel )
        {
            UnityEngine.Object.Destroy( vessel.gameObject );
        }
    }
}