using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadRebuilder : IDisposable
    {
        [Flags]
        public enum BuildSettings
        {
            None = 0,
            /// <summary>
            /// If specified, the rebuilder will include the immediate neighbors of the build area in the build process.
            /// </summary>
            /// <remarks>
            /// Use to clean edges of nodes that depend on the neighbors (e.g. smoothing).
            /// </remarks>
            IncludeNeighborsOfChanged = 1,

            Default = IncludeNeighborsOfChanged
        }

        private static IEqualityComparer<LODQuadTreeNode> _nodeEqualityComparer = new ValueLODQuadTreeNodeComparer();

        private Dictionary<LODQuadTreeNode, LODQuadRebuildData> _rebuildData;

        private MethodInfo[] _jobSchedulerPerJob; // Cached Schedule<JobType>( ... ) method with the job type for each job in sequence.
        private ILODQuadJob[] _jobs;
        private int[] _firstJobPerStage; // Index of the first job in the stage given by the index.

        public int StageCount => _firstJobPerStage.Length; // How many frames it will take from start to finish.

        /// <summary>
        /// Stage currently being built.
        /// </summary>
        public int LastStartedStage { get; private set; } = -1;
        /// <summary>
        /// Last stage that was completed.
        /// </summary>
        public int LastFinishedStage { get; private set; } = -1;

        public bool IsDone => LastFinishedStage == (StageCount - 1);

        LODQuadSphere _sphere;
        LODQuadMode _buildMode;
        BuildSettings _settings;

        private LODQuadRebuilder()
        {
        }

        static void Schedule<T>( ILODQuadJob[] jobs, JobHandle[] handles, int index ) where T : struct, ILODQuadJob
        {
            // Schedule a job from an array containing every job in some stage. The type T is the type of the job being scheduled.
            T jobToSchedule = (T)jobs[index];
            if( index == 0 )
                handles[index] = jobToSchedule.Schedule();
            else
                handles[index] = jobToSchedule.Schedule( handles[index - 1] );
            jobs[index] = jobToSchedule; // doesn't work without this line, idk why because it just copies the job instance, which should be the same as the one already there.
        }

        /// <summary>
        /// Builds the next stage.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Build()
        {
            if( IsDone )
            {
                throw new InvalidOperationException( $"{nameof( LODQuadRebuilder )}.{nameof( Build )} was called, but the rebuild is already finished." );
            }

            if( LastStartedStage == -1 )
            {
                foreach( var rQuad in _rebuildData.Values )
                {
                    rQuad.InitializeBuild( _jobs, _sphere );
                }
            }

            // FINISH CURRENT

            if( LastStartedStage > LastFinishedStage )
            {
                int currentStage = LastStartedStage;

                int firstJob = currentStage < 0
                    ? _firstJobPerStage[0]
                    : _firstJobPerStage[currentStage];
                int lastJob = (currentStage + 1) < StageCount
                    ? _firstJobPerStage[currentStage + 1]
                    : _jobs.Length;

                foreach( var rQuad in _rebuildData.Values )
                {
                    rQuad.handles[lastJob - 1].Complete();

                    foreach( var job in rQuad.jobs[firstJob..lastJob] )
                    {
                        try
                        {
                            job.Finish( rQuad );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Exception occurred while finishing a stage {currentStage} job of type '{job.GetType()}' on body '{_sphere.CelestialBody.ID}'." );
                            Debug.LogException( ex );
                        }

                        job.Dispose();
                    }
                }

                LastFinishedStage++;

                if( IsDone )
                {
                    foreach( var rQuad in _rebuildData.Values )
                    {
                        try
                        {
                            rQuad.FinalizeBuild( _sphere );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Exception occurred while finalizing the build of a quad on body '{_sphere.CelestialBody.ID}'." );
                            Debug.LogException( ex );
                        }

                        rQuad.Dispose();
                    }
                    return;
                }
            }

            // START NEXT

            if( LastStartedStage == LastFinishedStage )
            {
                int currentStage = LastStartedStage + 1;

                int firstJob = currentStage < 0
                    ? _firstJobPerStage[0]
                    : _firstJobPerStage[currentStage];
                int lastJob = (currentStage + 1) < StageCount
                    ? _firstJobPerStage[currentStage + 1]
                    : _jobs.Length;

                // Initialize everything first, because they might talk to each other.
                foreach( var rQuad in _rebuildData.Values )
                {
                    foreach( var job in rQuad.jobs[firstJob..lastJob] )
                    {
                        try
                        {
                            job.Initialize( rQuad, _rebuildData );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Exception occurred while initializing a stage {currentStage} job of type '{job.GetType()}' on body '{_sphere.CelestialBody.ID}'." );
                            Debug.LogException( ex );
                        }
                    }
                }

                // Schedule once they're done talking to each other.
                foreach( var rQuad in _rebuildData.Values )
                {
                    for( int i = firstJob; i < lastJob; i++ )
                    {
                        _jobSchedulerPerJob[i].Invoke( null, new object[] { rQuad.jobs, rQuad.handles, i } ); // Schedule<JobType>( rQuad.jobs, rQuad.handles, i );
                    }
                }

                LastStartedStage++;
            }
        }

        /// <summary>
        /// Gets the newly built quads.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IEnumerable<LODQuad> GetResults()
        {
            if( !IsDone )
            {
                throw new InvalidOperationException( $"{nameof( LODQuadRebuilder )}.{nameof( GetResults )} was called, but the rebuild hasn't been finished yet." );
            }

            return _rebuildData.Values.Select( q => q.Quad );
        }

        /// <summary>
        /// Waits for the remaining scheduled build jobs to finish (blocking the thread) and disposes rebuild the data.
        /// </summary>
        public void Dispose()
        {
            if( LastStartedStage > LastFinishedStage )
            {
                int currentStage = LastStartedStage;

                int lastJob = (currentStage + 1) < StageCount
                    ? _firstJobPerStage[currentStage + 1]
                    : _jobs.Length;

                foreach( var quad in _rebuildData.Values )
                {
                    quad.handles[lastJob - 1].Complete();
                }

                foreach( var rQuad in _rebuildData.Values )
                {
                    rQuad.Dispose();
                }
            }
        }


        /// <summary>
        /// Builds the meshes for the corresponding changes in the quad sphere.
        /// </summary>
        /// <param name="jobs">The jobs to use when building the meshes.</param>
        /// <returns>The rebuilder to use to rebuild the specified meshes.</returns>
        public static LODQuadRebuilder FromChanges( LODQuadSphere sphere, ILODQuadJob[][] jobsInStages, LODQuadTreeChanges changes, LODQuadMode buildMode, BuildSettings settings )
        {
            LODQuadRebuilder rebuilder = new LODQuadRebuilder();

            rebuilder._sphere = sphere;
            (rebuilder._jobs, rebuilder._firstJobPerStage) = ILODQuadJob.FilterJobs( jobsInStages, buildMode );
            rebuilder._buildMode = buildMode;
            rebuilder._settings = settings;


            MethodInfo method = typeof( LODQuadRebuilder ).GetMethod( nameof( LODQuadRebuilder.Schedule ), BindingFlags.Static | BindingFlags.NonPublic );

            rebuilder._jobSchedulerPerJob = new MethodInfo[rebuilder._jobs.Length];
            for( int i = 0; i < rebuilder._jobs.Length; i++ )
            {
                rebuilder._jobSchedulerPerJob[i] = method.MakeGenericMethod( rebuilder._jobs[i].GetType() );
            }

            rebuilder.SetQuadsToBuild( changes, settings );

            return rebuilder;
        }

        private void SetQuadsToBuild( LODQuadTreeChanges changes, BuildSettings settings )
        {
            var nodes = changes.GetNewNodes();

            if( settings.HasFlag( BuildSettings.IncludeNeighborsOfChanged ) )
            {
                nodes = nodes.Union( changes.GetDifferentNeighbors() );
            }

            Dictionary<LODQuadTreeNode, LODQuadRebuildData> rebuildData = new( _nodeEqualityComparer );
            foreach( var node in nodes )
            {
                if( !rebuildData.TryAdd( node, new LODQuadRebuildData( node ) ) )
                {
                    Debug.LogWarning( $"The rebuild of celestial body '{_sphere.CelestialBody.ID}' contained duplicate quads." );
                }
            }

            this._rebuildData = rebuildData;
        }
    }
}