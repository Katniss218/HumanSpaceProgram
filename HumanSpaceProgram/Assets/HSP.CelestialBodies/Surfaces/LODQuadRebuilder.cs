using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        private int _edgeSubdivisions;

        private LODQuadRebuildMode _buildMode;

        public bool IsDone => _nextStage == _firstJobPerStage.Length;

        private void InitializeBuild( LODQuadRebuildData r )
        {
            r.mesh = new Mesh();
            r.jobs = this._jobs;
            r.handles = new JobHandle[this._jobs.Length];

            int numberOfEdges = 1 << this._edgeSubdivisions; // Fast 2^n for integer types.
            int numberOfVertices = numberOfEdges + 1;
            r.resultVertices = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.Persistent );
            r.resultNormals = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.Persistent );
            r.resultUvs = new NativeArray<Vector2>( numberOfVertices * numberOfVertices, Allocator.Persistent );
            r.resultTriangles = new NativeArray<int>( (numberOfEdges * numberOfEdges) * 6, Allocator.Persistent );
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
                // 'await' the previous stage (should have an entire frame worth of time to finish)
                foreach( var quad in _quads )
                {
                    quad.handles[_jobBeingBuilt].Complete();

                    foreach( var job in quad.jobs[_firstJobPerStage[_nextStage - 1].._firstJobPerStage[_nextStage]] )
                        job.Finish( quad, this );
                }

                _jobBeingBuilt = -1;
            }

            // advance to the next stage, until there are stages to build.
            if( _jobBeingBuilt == -1 )
            {
                _nextStage++;

                if( IsDone )
                {
                    return;
                }

                //_jobBeingBuilt = _nextStage;

                // Initialize everything first, because they might talk to each other.
                foreach( var quad in _quads )
                {
#warning TODO - we somehow need to allow access to the meshes of quads that are not participating in the build process.
                    // this is safe, because another build can't be started while this one is running.

                    // access them from the mesh instances of the lodquads directly?

#warning TODO - should I calculate the normal from the mesh itself at all ahyway? maybe from the dydx on the heightmap?

                    foreach( var job in quad.jobs[_firstJobPerStage[_nextStage - 1].._firstJobPerStage[_nextStage]] )
                        job.Initialize( quad, this );
                }

                MethodInfo method = typeof( LODQuadRebuilder ).GetMethod( nameof( LODQuadRebuilder.Schedule ), BindingFlags.Static | BindingFlags.NonPublic );

                // Schedule once they're done talking to each other.
                foreach( var quad in _quads )
                {
                    for( int i = _firstJobPerStage[_nextStage - 1]; i < _firstJobPerStage[_nextStage]; i++ )
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
        public static LODQuadRebuilder FromChanges( ILODQuadJob[][] jobsInStages, LODQuadTreeChanges changes, LODQuadRebuildMode buildMode )
        {
            LODQuadRebuilder rebuilder = new LODQuadRebuilder();

            (rebuilder._jobs, rebuilder._firstJobPerStage) = ILODQuadJob.FilterJobs( jobsInStages, buildMode );
            rebuilder._buildMode = buildMode;
            rebuilder._quads = new (LODQuad quad, LODQuadRebuildData r)[changes.GetLeafNodes()];

            return rebuilder;
        }

        /// <summary>
        /// Builds the meshes for the entire quad sphere.
        /// </summary>
        /// <param name="jobs">The jobs to use when building the meshes.</param>
        /// <returns>The rebuilder to use to rebuild the specified meshes.</returns>
        public static LODQuadRebuilder FromWhole( ILODQuadJob[][] jobsInStages, LODQuadTree tree, LODQuadRebuildMode buildMode )
        {
            LODQuadRebuilder rebuilder = new LODQuadRebuilder();

            (rebuilder._jobs, rebuilder._firstJobPerStage) = ILODQuadJob.FilterJobs( jobsInStages, buildMode );
            rebuilder._buildMode = buildMode;
            rebuilder._quads = new (LODQuad quad, LODQuadRebuildData r)[tree.GetLeafNodes()];

            return rebuilder;
        }
    }
}