﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    [Flags]
    public enum LODQuadMode
    {
        /// <summary>
        /// Execute the job for visual meshes.
        /// </summary>
        Visual = 1,

        /// <summary>
        /// Execute the job for collision meshes.
        /// </summary>
        Collider = 2,

        /// <summary>
        /// Execute the job for both visual and collision meshes.
        /// </summary>
        VisualAndCollider = Visual | Collider
    }

    public enum LODQuadRebuildMode
    {
        /// <summary>
        /// Build the visual meshes.
        /// </summary>
        Visual = 1,

        /// <summary>
        /// Build the collision meshes.
        /// </summary>
        Collider = 2
    }

    public interface ILODQuadJob : IJob
    {
        /// <summary>
        /// Determines when the job should be executed.
        /// </summary>
        public LODQuadMode QuadMode { get; }

        /// <summary>
        /// Called on the main thread to initialize the job.
        /// </summary>
        public void Initialize( LODQuadRebuildData r );

        /// <summary>
        /// Called on the main thread to collect the result and dispose of the job.
        /// </summary>
        public void Finish( LODQuadRebuildData r );

        /// <summary>
        /// Clones the instance, copying the persistent settings.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS: <br/>
        /// - The members that are set in <see cref="Initialize"/> don't have to be copied.
        /// </remarks>
        /// <returns>The cloned job struct.</returns>
        public ILODQuadJob Clone();

        /// <summary>
        /// Filters jobs, returning only the ones used in the particular build mode.
        /// </summary>
        /// <param name="jobsInStages">The collection of jobs, split up into stages - jobsInStages[stage][job]</param>
        /// <returns>An array of all jobs matching the specified build mode, and an array containing the indices of the first job from each subsequent stage.</returns>
        public static (ILODQuadJob[] jobs, int[] firstJobPerStage) FilterJobs( ILODQuadJob[][] jobsInStages, LODQuadRebuildMode buildMode )
        {
            List<ILODQuadJob> jobs = new();
            List<int> firstJobPerStage = new();
            foreach( var stage in jobsInStages )
            {
                int stageStart = jobs.Count;
                bool anythingInStageAdded = false;

                foreach( var job in stage )
                {
                    if( ((int)job.QuadMode & (int)buildMode) != 0 )
                    {
                        jobs.Add( job );
                        anythingInStageAdded = true;
                    }
                }
                if( anythingInStageAdded )
                {
                    firstJobPerStage.Add( stageStart );
                }
            }

            return (jobs.ToArray(), firstJobPerStage.ToArray());
        }
    }
}