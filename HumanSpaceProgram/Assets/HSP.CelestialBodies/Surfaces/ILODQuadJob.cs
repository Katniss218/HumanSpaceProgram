using System;
using System.Collections.Generic;
using Unity.Jobs;

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

    /// <summary>
    /// Jobs are used to create/modify the data of the quad (vertex positions, normals, etc) as it's being built. <br/>
    /// Implement this interface to create a custom job type.
    /// </summary>
    public interface ILODQuadJob : IJob, IDisposable
    {
        /// <summary>
        /// Determines which LOD sphere modes the job should be executed for.
        /// </summary>
        public LODQuadMode QuadMode { get; }

        // It would be nice to validate this IReadOnlyDictionary somehow,
        // but it might be genuinely useful to allow syncing for jobs that don't begin a stage - if they need data from previous stage.
        // It means that whoever sets up planets needs to be very careful where they place jobs.
        /// <summary>
        /// Called on the main thread to initialize the job.
        /// </summary>
        /// <param name="r">The rebuild data of the current quad.</param>
        /// <param name="rAll">A dictionary containing the rest of the quads being rebuilt. USE WITH CARE, ONLY SYNCHRONIZED WITH DATA FROM THE PREVIOUS STAGE.</param>
        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData rAdditional );

        /// <summary>
        /// Called on the main thread to collect the result.
        /// </summary>
        /// <param name="r">The rebuild data of the current quad.</param>
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
        public static (ILODQuadJob[] jobs, int[] firstJobPerStage) FilterJobs( ILODQuadJob[][] jobsInStages, LODQuadMode buildMode )
        {
            List<ILODQuadJob> jobs = new();
            List<int> firstJobPerStage = new();
            foreach( var stage in jobsInStages )
            {
                int stageStart = jobs.Count;
                bool anythingInStageAdded = false;

                foreach( var job in stage )
                {
                    // All jobs that intersect the desired build mode.
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

        /// <summary>
        /// Clones the job instances into a new array
        /// </summary>
        public static ILODQuadJob[][] CopyJobsWithValidation( ILODQuadJob[][] jobs )
        {
            if( jobs == null )
            {
                throw new ArgumentNullException( nameof( jobs ), $"Jobs can't be null." );
            }

            ILODQuadJob[][] jobsInStages = new ILODQuadJob[jobs.Length][];
            for( int i = 0; i < jobs.Length; i++ )
            {
                if( jobs[i] == null )
                    throw new ArgumentNullException( nameof( jobs ), $"The stage {i} was null." );

                jobsInStages[i] = new ILODQuadJob[jobs[i].Length];
                for( int j = 0; j < jobs[i].Length; j++ )
                {
                    if( jobs[i][j] == null )
                        throw new ArgumentNullException( nameof( jobs ), $"The job {j} in stage {i} was null." );

                    jobsInStages[i][j] = jobs[i][j].Clone();
                }
            }
            return jobsInStages;
        }
    }
}