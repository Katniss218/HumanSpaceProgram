using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    /// <summary>
    /// A job that constructs the base mesh for the terrain.
    /// </summary>
    public struct MakeQuadMesh_Job : IJob
    {
        public int subdivisions;
        public float radius;
        public Vector2 center;
        public int lN;
        public Vector3 origin;

        //public Mesh resultMesh;

        public NativeArray<Vector3> vertices;
        public NativeArray<Vector3> normals;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> triangles;

        float size;

        int numberOfEdges;
        int numberOfVertices;
        float edgeLength;
        float minX;
        float minY;

        public void Initialize()
        {
            size = LODQuadUtils.GetSize( lN );

            numberOfEdges = 1 << subdivisions; // Fast 2^n for integer types.
            numberOfVertices = numberOfEdges + 1;
            edgeLength = size / numberOfEdges; // size represents the edge length of the original square, twice the radius.
            minX = center.x - (size / 2f); // center minus half the edge length of the cube.
            minY = center.y - (size / 2f);

            vertices = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.TempJob );
            normals = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.TempJob );
            uvs = new NativeArray<Vector2>( numberOfVertices * numberOfVertices, Allocator.TempJob );
            triangles = new NativeArray<int>( (numberOfEdges * numberOfEdges) * 6, Allocator.TempJob );
        }

        public void Execute()
        {
            GenerateCubeSphereFace();
        }

        /// <summary>
        /// The method that generates the PQS mesh projected onto a sphere of the specified radius, with its origin at the center of the cube projected onto the same sphere.
        /// </summary>
        /// <param name="lN">How many times this mesh was subdivided (l0, l1, l2, ...).</param>
        public void GenerateCubeSphereFace()
        {
            // The origin of a valid, the center will never be at any of the edges of its ancestors, and will always be at the point where the inner edges of its direct children meet.

            QuadSphereFace face = QuadSphereFaceEx.FromVector( origin.normalized );

            if( numberOfVertices > 65535 )
            {
                // technically wrong, since Mesh.indexFormat can be switched to 32 bit, but i'll leave this for now. Meshes don't have to be over that value anyway because laggy and big and far away.
                throw new ArgumentOutOfRangeException( $"Unity's Mesh can contain at most 65535 vertices (16-bit buffer). Tried to create a Mesh with {numberOfVertices}." );
            }

            for( int i = 0; i < numberOfVertices; i++ )
            {
                for( int j = 0; j < numberOfVertices; j++ )
                {
                    int index = (i * numberOfEdges) + i + j;

                    float quadX = (i * edgeLength) + minX;
                    float quadY = (j * edgeLength) + minY;

                    Vector3Dbl posD = face.GetSpherePointDbl( quadX, quadY );
#warning TODO - LOD Terrain edge interpolation.

                    Vector3 unitSpherePos = (Vector3)posD;

                    uvs[index] = new Vector2( quadX * 0.5f + 0.5f, quadY * 0.5f + 0.5f );
                    vertices[index] = (Vector3)((posD * radius) - origin);

                    // Normals after displacing by heightmap will need to be calculated by hand instead of with RecalculateNormals() to avoid seams not matching up.
                    // normals can be calculated by adding the normals of each face to its vertices, then normalizing.
                    // - this will compute smooth VERTEX normals!!
                    normals[index] = unitSpherePos;
                }
            }

            int triIndex = 0;
            for( int i = 0; i < numberOfEdges; i++ )
            {
                for( int j = 0; j < numberOfEdges; j++ )
                {
                    int index = (i * numberOfEdges) + i + j;

                    //   C - D
                    //   | / |
                    //   A - B

                    // Adding numberOfVertices makes it skip to the next row (number of vertices is 1 higher than edges).
                    triangles[triIndex  ]= index; // A
                    triangles[triIndex+1] = index + numberOfVertices + 1; // D
                    triangles[triIndex+2] = index + numberOfVertices; // C

                    triangles[triIndex+3] = index; // A
                    triangles[triIndex+4] = index + 1; // B
                    triangles[triIndex+5] = index + numberOfVertices + 1; // D

                    triIndex += 6;
                }
            }

            // Sadly can't calculate tangents properly easily here.
            // custom method is fucky, and a Mesh object can't be created here.
        }
    }
}