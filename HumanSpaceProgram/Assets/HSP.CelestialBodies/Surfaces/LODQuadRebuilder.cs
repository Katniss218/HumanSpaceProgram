using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadRebuildData // describes a *single* quad that's being created.
    {
        public LODQuad quad;
        public LODQuadTreeNode node;
        public Mesh mesh;

        public ILODQuadJob[] jobs;
        public JobHandle[] handles;

        public double radius;
        public int numberOfVertices; // per side
        public int numberOfEdges;
        public NativeArray<Vector3> resultVertices;
        public NativeArray<Vector3> resultNormals;
        public NativeArray<Vector2> resultUvs;
        public NativeArray<int> resultTriangles;
    }

    public class LODQuadRebuilder
    {
        private LODQuadRebuildData[] _quads;

        private ILODQuadJob[] _jobs;
        private int[] _firstJobPerStage; // index of the first job in the given stage

        private int _stageCount => _firstJobPerStage.Length; // how many frames it will take from start to finish.

        private int _nextStage = 0;
        private int _jobBeingBuilt = -1; // index of the last job in sequence in the stage that's currently being built.

        LODQuadSphere sphere;
        private LODQuadRebuildMode _buildMode;

        public bool IsDone => _nextStage == (_firstJobPerStage.Length + 1);

        private void InitializeBuild( LODQuadRebuildData r )
        {
            r.mesh = new Mesh();
            r.jobs = this._jobs;
            r.handles = new JobHandle[this._jobs.Length];

            int numberOfEdges = 1 << sphere.EdgeSubdivisions; // Fast 2^n for integer types.
            int numberOfVertices = numberOfEdges + 1;
            r.numberOfVertices = numberOfVertices;
            r.numberOfEdges = numberOfEdges;
            r.radius = sphere.CelestialBody.Radius;
            r.resultVertices = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.Persistent );
            r.resultNormals = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.Persistent );
            r.resultUvs = new NativeArray<Vector2>( numberOfVertices * numberOfVertices, Allocator.Persistent );
            r.resultTriangles = new NativeArray<int>( (numberOfEdges * numberOfEdges) * 6, Allocator.Persistent );
        }

        private void FinalizeBuild( LODQuadRebuildData r )
        {
            r.quad = LODQuad.CreateInactive( sphere, r.node, r.mesh );
            r.resultVertices.Dispose();
            r.resultNormals.Dispose();
            r.resultUvs.Dispose();
            r.resultTriangles.Dispose();
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

            if( _nextStage == 0 )
            {
                foreach( var quad in _quads )
                {
                    InitializeBuild( quad );
                }
            }

            if( _jobBeingBuilt != -1 )
            {
                int stageStart = _nextStage == 0 ? _firstJobPerStage[0] : _firstJobPerStage[_nextStage - 1];
                int stageEnd = (_firstJobPerStage.Length > _nextStage) ? _firstJobPerStage[_nextStage] : _jobs.Length;

                // 'await' the previous stage (should have an entire frame worth of time to finish)
                foreach( var quad in _quads )
                {
#warning TODO - some jobs aren't finished or something?
                    quad.handles[stageEnd - 1].Complete();

                    foreach( var job in quad.jobs[stageStart..stageEnd] )
                        job.Finish( quad );
                }

                _jobBeingBuilt = -1;
            }

            // advance to the next stage, until there are stages to build.
            if( _jobBeingBuilt == -1 )
            {
                _nextStage++;

                if( IsDone )
                {
                    foreach( var quad in _quads )
                    {
                        FinalizeBuild( quad );
                    }
                    return;
                }

                _jobBeingBuilt = _nextStage;

                int stageStart = _nextStage == 0 ? _firstJobPerStage[0] : _firstJobPerStage[_nextStage - 1];
                int stageEnd = (_firstJobPerStage.Length > _nextStage) ? _firstJobPerStage[_nextStage] : _jobs.Length;

                // Initialize everything first, because they might talk to each other.
                foreach( var quad in _quads )
                {
                    foreach( var job in quad.jobs[stageStart..stageEnd] )
                        job.Initialize( quad );
                }

                MethodInfo method = typeof( LODQuadRebuilder ).GetMethod( nameof( LODQuadRebuilder.Schedule ), BindingFlags.Static | BindingFlags.NonPublic );

                // Schedule once they're done talking to each other.
                foreach( var quad in _quads )
                {
                    for( int i = stageStart; i < stageEnd; i++ )
                    {
                        // Schedule<MakeQuadMesh_Job>( quad._jobsPerStage[_stageBeingBuilt][i], quad.handlesPerStage[_stageBeingBuilt][i], i );

#warning TODO - can be optimized because every quad will have the same types in parallel
                        Type jobType = _jobs[i].GetType();
                        method.MakeGenericMethod( jobType ).Invoke( null, new object[] { quad.jobs, quad.handles, i } );
                    }
                }

                _nextStage++;
            }
        }

        public IEnumerable<LODQuad> GetResults()
        {
            if( !IsDone )
            {
                throw new InvalidOperationException( $"{nameof( LODQuadRebuilder )}.{nameof( GetResults )} was called, but the rebuild hasn't been finished yet." );
            }

            return _quads.Select( q => q.quad );
        }


        /// <summary>
        /// Builds the meshes for the corresponding changes in the quad sphere.
        /// </summary>
        /// <param name="jobs">The jobs to use when building the meshes.</param>
        /// <returns>The rebuilder to use to rebuild the specified meshes.</returns>
        public static LODQuadRebuilder FromChanges( LODQuadSphere sphere, ILODQuadJob[][] jobsInStages, LODQuadTreeChanges changes, LODQuadRebuildMode buildMode )
        {
            LODQuadRebuilder rebuilder = new LODQuadRebuilder();

            rebuilder.sphere = sphere;
            (rebuilder._jobs, rebuilder._firstJobPerStage) = ILODQuadJob.FilterJobs( jobsInStages, buildMode );
            rebuilder._buildMode = buildMode;
            rebuilder._quads = GetQuadsToBuild( changes );

            return rebuilder;
        }

        /// <summary>
        /// Builds the meshes for the entire quad sphere.
        /// </summary>
        /// <param name="jobs">The jobs to use when building the meshes.</param>
        /// <returns>The rebuilder to use to rebuild the specified meshes.</returns>
        /*public static LODQuadRebuilder FromWhole( double radius, ILODQuadJob[][] jobsInStages, LODQuadTree tree, LODQuadRebuildMode buildMode )
        {
            LODQuadRebuilder rebuilder = new LODQuadRebuilder();
        
            rebuilder.radius = radius;
            (rebuilder._jobs, rebuilder._firstJobPerStage) = ILODQuadJob.FilterJobs( jobsInStages, buildMode );
            rebuilder._buildMode = buildMode;
            rebuilder._quads = GetQuadsToBuild( tree );

            return rebuilder;
        }*/

        private static LODQuadRebuildData[] GetQuadsToBuild( LODQuadTreeChanges changes )
        {
            LODQuadRebuildData[] quads = new LODQuadRebuildData[(changes.newRoots?.Length ?? 0) + (changes.subdivided?.Count * 4 ?? 0)];

            int i = 0;
            if( changes.newRoots != null )
            {
                int end = i + changes.newRoots.Length;
                for( ; i < end; i++ )
                {
                    quads[i] = new LODQuadRebuildData()
                    {
                        node = changes.newRoots[i],
                    };
                }
            }

            if( changes.subdivided != null )
            {
                int end = i + changes.subdivided.Count;
                for( ; i < end; i++ )
                {
                    quads[(i * 4)] = new LODQuadRebuildData()
                    {
                        node = changes.subdivided[i].xnyn,
                    };
                    quads[(i * 4) + 1] = new LODQuadRebuildData()
                    {
                        node = changes.subdivided[i].xpyn,
                    };
                    quads[(i * 4) + 2] = new LODQuadRebuildData()
                    {
                        node = changes.subdivided[i].xnyp,
                    };
                    quads[(i * 4) + 3] = new LODQuadRebuildData()
                    {
                        node = changes.subdivided[i].xpyp,
                    };
                }
            }

            return quads;
        }
    }
}