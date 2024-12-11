using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// Contains shared data about a single quad mesh being created.
    /// </summary>
    public class LODQuadRebuildData : IDisposable
    {
        /// <summary>
        /// The quad being build. <br/>
        /// Only assigned after the build process is finished.
        /// </summary>
        public LODQuad Quad { get; internal set; }

        /// <summary>
        /// The tree node associated with this quad.
        /// </summary>
        public LODQuadTreeNode Node { get; }

        /// <summary>
        /// The new mesh instance associated with this quad.
        /// </summary>
        public Mesh Mesh { get; private set; }

        /// <summary>
        /// The radius of the celestial body.
        /// </summary>
        public CelestialBody CelestialBody { get; private set; }

        /// <summary>
        /// The number of vertices per side.
        /// </summary>
        public int SideVertices { get; private set; }
        /// <summary>
        /// The number of edges per side (1 - number of vertices).
        /// </summary>
        public int SideEdges { get; private set; }

        /// <summary>
        /// The shared array with vertex positions.
        /// </summary>
        public NativeArray<Vector3> ResultVertices { get; private set; }

        /// <summary>
        /// The shared array with vertex normals.
        /// </summary>
        public NativeArray<Vector3> ResultNormals { get; private set; }

        /// <summary>
        /// The shared array with vertex UVs.
        /// </summary>
        public NativeArray<Vector2> ResultUVs { get; private set; }

        /// <summary>
        /// The shared array with triangle indices.
        /// </summary>
        public NativeArray<int> ResultTriangles { get; private set; }

        internal ILODQuadJob[] jobs { get; private set; }
        internal JobHandle[] handles { get; private set; }

        public LODQuadRebuildData( LODQuadTreeNode node )
        {
            this.Node = node;
        }

        public void InitializeBuild( ILODQuadJob[] jobs, LODQuadSphere sphere )
        {
            this.jobs = new ILODQuadJob[jobs.Length];
            this.handles = new JobHandle[jobs.Length];
            for( int i = 0; i < jobs.Length; i++ )
            {
                this.jobs[i] = jobs[i].Clone();
            }

            Mesh = new Mesh();

            CelestialBody = sphere.CelestialBody;
            SideEdges = 1 << sphere.EdgeSubdivisions; // Fast 2^n for integer types.
            SideVertices = SideEdges + 1;

            ResultVertices = new NativeArray<Vector3>( SideVertices * SideVertices, Allocator.Persistent );
            ResultNormals = new NativeArray<Vector3>( SideVertices * SideVertices, Allocator.Persistent );
            ResultUVs = new NativeArray<Vector2>( SideVertices * SideVertices, Allocator.Persistent );
            ResultTriangles = new NativeArray<int>( (SideEdges * SideEdges) * 6, Allocator.Persistent );
        }

        public void FinalizeBuild( LODQuadSphere sphere )
        {
            Quad = LODQuad.CreateInactive( sphere, Node, Mesh );
        }

        public void Dispose()
        {
            ResultVertices.Dispose();
            ResultNormals.Dispose();
            ResultUVs.Dispose();
            ResultTriangles.Dispose();
        }
    }

    public class LODQuadRebuildAdditionalData
    {
        public struct Entry
        {
            public readonly bool hasNew;
            public readonly bool hasOld;

            public readonly LODQuadRebuildData @new;
            public readonly Mesh.MeshData old;

            public Entry( Mesh.MeshData old )
            {
                this.hasNew = false;
                this.hasOld = true;
                this.@new = null;
                this.old = old;
            }

            public Entry( LODQuadRebuildData @new )
            {

                this.hasNew = true;
                this.hasOld = false;
                this.@new = @new;
                this.old = default;
            }
            public Entry( LODQuadRebuildData @new, Mesh.MeshData old )
            {
                this.hasNew = true;
                this.hasOld = true;
                this.@new = @new;
                this.old = old;
            }
        }

        public IReadOnlyDictionary<LODQuadTreeNode, Entry> allQuads;

        public LODQuadRebuildAdditionalData( Dictionary<LODQuadTreeNode, Entry> quads )
        {
            this.allQuads = quads;
        }
    }
}