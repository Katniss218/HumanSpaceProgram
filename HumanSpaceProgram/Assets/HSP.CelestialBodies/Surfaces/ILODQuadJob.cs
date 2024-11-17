using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public interface ILODQuadJob : IJob
    {
        /// <summary>
        /// Initialize is called on the main thread to initialize the job.
        /// </summary>
        /// <param name="quad"></param>
        /// <param name="mesh"></param>
        public void Initialize( LODQuad quad, Mesh mesh );

        /// <summary>
        /// Finish is called on the main thread to collect the result and dispose of the job.
        /// </summary>
        /// <param name="quad"></param>
        /// <param name="mesh"></param>
        public void Finish( LODQuad quad, Mesh mesh );
    }
}
