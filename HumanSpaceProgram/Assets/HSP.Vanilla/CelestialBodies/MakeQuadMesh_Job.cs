using HSP.CelestialBodies.Surfaces;
using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    /// <summary>
    /// A job that constructs the base mesh for the terrain.
    /// </summary>
    public struct MakeQuadMesh_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;
        float size;
        float edgeLength;
        float minX;
        float minY;
        Direction3D face;

        int sideEdges;
        int sideVertices;

        NativeArray<Vector3Dbl> resultVertices;
        NativeArray<Vector3> resultNormals;
        NativeArray<Vector2> resultUvs;
        NativeArray<int> resultTriangles;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData _ )
        {
#warning TODO - keep the verts in body space (as vector3dbl), turn into mesh-space when finalizing.
            radius = (float)r.CelestialBody.Radius;
            origin = r.Node.SphereCenter * radius;
            size = r.Node.Size;
            face = r.Node.Face;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            edgeLength = size / sideEdges; // size represents the edge length of the original square, twice the radius.
            minX = r.Node.FaceCenter.x - (size / 2f); // center minus half the edge length of the cube.
            minY = r.Node.FaceCenter.y - (size / 2f);

            resultVertices = r.ResultVertices;
            resultNormals = r.ResultNormals;
            resultUvs = r.ResultUVs;
            resultTriangles = r.ResultTriangles;
        }

        public void Finish( LODQuadRebuildData r )
        {
            //r.ResultMesh.SetVertices( resultVertices );
            //r.ResultMesh.SetNormals( resultNormals );
            //r.ResultMesh.SetUVs( 0, resultUvs );
            //r.ResultMesh.SetTriangles( resultTriangles.ToArray(), 0 );
        }

        public void Dispose()
        {
        }

        public ILODQuadJob Clone()
        {
            return new MakeQuadMesh_Job();
        }

        public void Execute() // Called by Unity from a job thread.
        {
            GenerateCubeSphereFace();
        }

        int GetIndex( int x, int y )
        {
            return (x * sideEdges) + x + y;
        }

        /// <summary>
        /// The method that generates the PQS mesh projected onto a sphere of the specified radius, with its origin at the center of the cube projected onto the same sphere.
        /// </summary>
        /// <param name="lN">How many times this mesh was subdivided (l0, l1, l2, ...).</param>
        public void GenerateCubeSphereFace()
        {
            // The origin of a valid, the center will never be at any of the edges of its ancestors, and will always be at the point where the inner edges of its direct children meet.

            if( sideVertices > 65535 )
            {
                // technically wrong, since Mesh.indexFormat can be switched to 32 bit, but i'll leave this for now. Meshes don't have to be over that value anyway because laggy and big and far away.
                throw new ArgumentOutOfRangeException( $"Unity's Mesh can contain at most 65535 vertices (16-bit buffer). Tried to create a Mesh with {sideVertices}." );
            }

            int triIndex = 0;
            for( int x = 0; x < sideEdges; x++ )
            {
                for( int y = 0; y < sideEdges; y++ )
                {
                    int index = GetIndex( x, y );

                    //   C - D
                    //   | / |
                    //   A - B

                    // Adding numberOfVertices makes it skip to the next row (number of vertices is 1 higher than edges).
                    resultTriangles[triIndex] = index; // A
                    resultTriangles[triIndex + 1] = index + sideVertices + 1; // D
                    resultTriangles[triIndex + 2] = index + sideVertices; // C

                    resultTriangles[triIndex + 3] = index; // A
                    resultTriangles[triIndex + 4] = index + 1; // B
                    resultTriangles[triIndex + 5] = index + sideVertices + 1; // D

                    triIndex += 6;
                }
            }

            for( int x = 0; x < sideVertices; x++ )
            {
                for( int y = 0; y < sideVertices; y++ )
                {
                    int index = GetIndex( x, y );

                    float quadX = (x * edgeLength) + minX; // This might need to be turned into a double perhaps (for large bodies with lots of subdivs).
                    float quadY = (y * edgeLength) + minY;

                    Vector3Dbl posD = face.GetSpherePointDbl( quadX, quadY );

                    resultVertices[index] = posD * radius;

                    // INFO - 'lerping' it like that introduces stretch, the cubemap should be counter-stretched to cancel it.
                    resultUvs[index] = new Vector2( quadX * 0.5f + 0.5f, quadY * 0.5f + 0.5f );

                    resultNormals[index] = (Vector3)posD;
                }
            }
        }
    }
}