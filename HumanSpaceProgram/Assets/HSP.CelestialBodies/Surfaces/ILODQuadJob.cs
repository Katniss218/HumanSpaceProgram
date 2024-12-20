using System;
using Unity.Jobs;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// A job is returned by a modifier to do the work on the separate thread.
    /// </summary>
    public interface ILODQuadJob : IJob, IDisposable
    {
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
    }
}