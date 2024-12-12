using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public sealed class LODQuadRebuilder : IDisposable
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
            IncludeNodesWithChangedNeighbors = 1,

            Default = IncludeNodesWithChangedNeighbors
        }

        private static IEqualityComparer<LODQuadTreeNode> _nodeEqualityComparer = new ValueLODQuadTreeNodeComparer();

        private LODQuadRebuildData[] _rebuild;
        private LODQuadRebuildAdditionalData _additionalData;

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

        public LODQuadRebuilder( LODQuadSphere sphere, LODQuadTreeChanges changes, BuildSettings settings = BuildSettings.Default )
        {
            this._sphere = sphere;
            (this._jobs, this._firstJobPerStage) = sphere.GetJobsForBuild();
            this._buildMode = sphere.Mode;
            this._settings = settings;

            MethodInfo method = typeof( LODQuadRebuilder ).GetMethod( nameof( LODQuadRebuilder.Schedule ), BindingFlags.Static | BindingFlags.NonPublic );

            this._jobSchedulerPerJob = new MethodInfo[this._jobs.Length];
            for( int i = 0; i < this._jobs.Length; i++ )
            {
                this._jobSchedulerPerJob[i] = method.MakeGenericMethod( this._jobs[i].GetType() );
            }

            this.InitializeRebuildData( sphere, changes, settings );
        }

        private void InitializeRebuildData( LODQuadSphere sphere, LODQuadTreeChanges changes, BuildSettings settings )
        {
            IReadOnlyDictionary<LODQuadTreeNode, LODQuadRebuildData> existingNodes = sphere.CurrentQuads;
            IEnumerable<LODQuadTreeNode> newNodes = changes.GetNewNodes();

            if( settings.HasFlag( BuildSettings.IncludeNodesWithChangedNeighbors ) )
            {
                newNodes = newNodes.Union( changes.GetDifferentNeighbors() );
            }

            Dictionary<LODQuadTreeNode, LODQuadRebuildData> rebuildDict = new();
            foreach( var node in newNodes )
            {
                if( !rebuildDict.TryAdd( node, new LODQuadRebuildData( node ) ) )
                {
                    Debug.LogWarning( $"The rebuild of celestial body '{_sphere.CelestialBody.ID}' contained duplicate quads." );
                }
            }

            Dictionary<LODQuadTreeNode, LODQuadRebuildAdditionalData.Entry> rebuildData = new( _nodeEqualityComparer );
            foreach( var node in existingNodes )
            {
                var entry = new LODQuadRebuildAdditionalData.Entry( null, node.Value );

                if( !rebuildData.TryAdd( node.Key, entry ) )
                {
                    Debug.LogWarning( $"The rebuild of celestial body '{_sphere.CelestialBody.ID}' contained duplicate quads." );
                }
            }

            foreach( var node in newNodes )
            {
                if( rebuildData.TryGetValue( node, out var data ) )
                {
                    // add new info and replace in dict.
                    data = new LODQuadRebuildAdditionalData.Entry( rebuildDict[node], data.oldR );
                    rebuildData[node] = data;
                }
                else
                {
                    data = new LODQuadRebuildAdditionalData.Entry( rebuildDict[node], null );
                    rebuildData[node] = data;
                }
            }

            this._additionalData = new LODQuadRebuildAdditionalData( rebuildData );
            this._rebuild = rebuildDict.Values.ToArray();
        }


        /// <summary>
        /// Schedule a job from the array containing all jobs.
        /// </summary>
        /// <typeparam name="T">The type of the job being scheduled.</typeparam>
        private static void Schedule<T>( ILODQuadJob[] jobs, JobHandle[] handles, int index ) where T : struct, ILODQuadJob
        {
            T jobToSchedule = (T)jobs[index];
            if( index == 0 )
                handles[index] = jobToSchedule.Schedule();
            else
                handles[index] = jobToSchedule.Schedule( handles[index - 1] );
            jobs[index] = jobToSchedule; // doesn't work without re-setting the job, idk why.
        }

        /// <summary>
        /// Waits for the current stage to finish building (if any), and schedules the next stage to start building (if any).
        /// </summary>
        /// <remarks>
        /// Check if the build has been completed (<see cref="IsDone"/>) before calling this method.
        /// </remarks>
        /// <exception cref="InvalidOperationException"></exception>
        public void Build()
        {
            if( IsDone )
            {
                throw new InvalidOperationException( $"{nameof( LODQuadRebuilder )}.{nameof( Build )} was called, but the rebuild is already finished." );
            }

            if( LastStartedStage == -1 )
            {
                foreach( var rQuad in _rebuild )
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

                foreach( var rQuad in _rebuild )
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
                    foreach( var rQuad in _rebuild )
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

                        //rQuad.Dispose(); moved to lodsphere, done only after the quad is also destroyed.
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
                foreach( var rQuad in _rebuild )
                {
                    foreach( var job in rQuad.jobs[firstJob..lastJob] )
                    {
                        try
                        {
                            job.Initialize( rQuad, _additionalData );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Exception occurred while initializing a stage {currentStage} job of type '{job.GetType()}' on body '{_sphere.CelestialBody.ID}'." );
                            Debug.LogException( ex );
                        }
                    }
                }

                // Schedule once they're done talking to each other.
                foreach( var rQuad in _rebuild )
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
        /// <remarks>
        /// Check if the build has been completed (<see cref="IsDone"/>) before calling this method.
        /// </remarks>
        /// <exception cref="InvalidOperationException"></exception>
        public IEnumerable<LODQuadRebuildData> GetResults()
        {
            if( !IsDone )
            {
                throw new InvalidOperationException( $"{nameof( LODQuadRebuilder )}.{nameof( GetResults )} was called, but the rebuild hasn't been finished yet." );
            }

            return _rebuild.Select( q => q );
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

                foreach( var quad in _rebuild )
                {
                    quad.handles[lastJob - 1].Complete();
                }

                foreach( var rQuad in _rebuild )
                {
                    rQuad.Dispose();
                }
            }
        }
    }
}