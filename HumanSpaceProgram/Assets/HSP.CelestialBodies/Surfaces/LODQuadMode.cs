using System;

namespace HSP.CelestialBodies.Surfaces
{
    [Flags]
    public enum LODQuadMode
    {
        /// <summary>
        /// Execute the LOD quad modifier for visual meshes.
        /// </summary>
        Visual = 1,

        /// <summary>
        /// Execute the LOD quad modifier for collision meshes.
        /// </summary>
        Collider = 2,

        /// <summary>
        /// Execute the LOD quad modifier for both visual and collision meshes.
        /// </summary>
        VisualAndCollider = Visual | Collider
    }
}