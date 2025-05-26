using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Effects.Meshes
{
    public static class MeshCloner
    {
        /// <summary>
        /// Creates an exact copy of the given Mesh, including vertices, normals, tangents,
        /// UVs, colors, submeshes, bind poses, bone weights, and blend shapes.
        /// </summary>
        /// <param name="source">The Mesh to copy.</param>
        /// <returns>A new Mesh instance that is a deep copy of the source.</returns>
        public static Mesh GetDeepCopy( this Mesh source )
        {
            if( source == null )
                throw new ArgumentNullException( nameof( source ) );

            Mesh mesh = new Mesh()
            {
                name = source.name,
                indexFormat = source.indexFormat,
                vertices = source.vertices,
                normals = source.normals,
                tangents = source.tangents,
                colors = source.colors,
                colors32 = source.colors32
            };

            // Unity supports up to 8 UV channels.
            for( int channel = 0; channel < 8; channel++ )
            {
                List<Vector4> uvs = new();
                try
                {
                    source.GetUVs( channel, uvs );
                    if( uvs != null && uvs.Count > 0 )
                    {
                        mesh.SetUVs( channel, uvs );
                    }
                }
                catch( ArgumentException ex )
                {
                    // Channel not used, ignore
                }
            }

            // Copy triangles/indices for each submesh
            mesh.subMeshCount = source.subMeshCount;
            for( int i = 0; i < source.subMeshCount; i++ )
            {
                mesh.SetIndices( source.GetIndices( i ), source.GetTopology( i ), i );
            }

            // Copy skinning data
            mesh.bindposes = source.bindposes;
            mesh.boneWeights = source.boneWeights;

            // Copy blend shapes
            int blendShapeCount = source.blendShapeCount;
            for( int shapeIndex = 0; shapeIndex < blendShapeCount; shapeIndex++ )
            {
                string shapeName = source.GetBlendShapeName( shapeIndex );
                int frameCount = source.GetBlendShapeFrameCount( shapeIndex );
                for( int frameIndex = 0; frameIndex < frameCount; frameIndex++ )
                {
                    float frameWeight = source.GetBlendShapeFrameWeight( shapeIndex, frameIndex );
                    Vector3[] deltaVertices = new Vector3[source.vertexCount];
                    Vector3[] deltaNormals = new Vector3[source.vertexCount];
                    Vector3[] deltaTangents = new Vector3[source.vertexCount];
                    source.GetBlendShapeFrameVertices( shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents );
                    mesh.AddBlendShapeFrame( shapeName, frameWeight, deltaVertices, deltaNormals, deltaTangents );
                }
            }

            mesh.bounds = source.bounds;

#if UNITY_EDITOR
            mesh.hideFlags = source.hideFlags;
#endif

            return mesh;
        }
    }
}
