using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    [Flags]
    public enum LODQuadJobMode
    {
        /// <summary>
        /// Execute the job for visual meshes.
        /// </summary>
        Visual,

        /// <summary>
        /// Execute the job for collision meshes.
        /// </summary>
        Collider,

        /// <summary>
        /// Execute the job for both visual and collision meshes.
        /// </summary>
        VisualAndCollider = Visual | Collider
    }

    public interface ILODQuadJob : IJob
    {
        /// <summary>
        /// Determines when the job should be executed.
        /// </summary>
        //public LODQuadJobMode QuadMode { get; }

        /// <summary>
        /// Initialize is called on the main thread to initialize the job.
        /// </summary>
        /// <param name="quad"></param>
        /// <param name="mesh"></param>
        public void Initialize( LODQuad quad, LODQuad.State.Rebuild r );

        /// <summary>
        /// Finish is called on the main thread to collect the result and dispose of the job.
        /// </summary>
        /// <param name="quad"></param>
        /// <param name="mesh"></param>
        public void Finish( LODQuad quad, LODQuad.State.Rebuild r );
    }
}
