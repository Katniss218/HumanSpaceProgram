using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies
{
    /// <summary>
    /// Retrieves a collection of points of interest (POIs) in the game world, that the <see cref="Surfaces.LODQuadSphere"/> will subdivide towards.
    /// </summary>
    public interface IPOIGetter
    {
        /// <summary>
        /// Gets the collection of POIs.
        /// </summary>
        /// <remarks>
        /// The returned collection can have a varying size, as well as be empty.
        /// </remarks>
        IEnumerable<Vector3Dbl> GetPOIs();
    }
}
