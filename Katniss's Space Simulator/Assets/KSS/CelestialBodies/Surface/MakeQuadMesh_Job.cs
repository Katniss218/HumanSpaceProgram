using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace KSS.CelestialBodies.Surface
{
    /// <summary>
    /// A job that constructs the base mesh for the terrain.
    /// </summary>
    public struct MakeQuadMesh_Job : IJob
    {
        int subdivisions;
        double radius;
        Vector2 center;
        int lN;
        Vector3 origin;

        NativeArray<Vector3> resultVertices;
        NativeArray<Vector3> resultNormals;
        NativeArray<Vector2> resultUvs;
        NativeArray<int> resultTriangles;

        float size;

        int numberOfEdges;
        int numberOfVertices;
        float edgeLength;
        float minX;
        float minY;

        NativeArray<int> edgeSubdivisionRelative;

        public void Initialize( LODQuad quad )
        {
            // Initialize is called on the main thread to initialize the job.

            subdivisions = quad.EdgeSubdivisions;
            radius = (float)quad.CelestialBody.Radius;
            center = quad.Node.Center;
            lN = quad.SubdivisionLevel;
            origin = quad.transform.localPosition;

            size = LODQuadTree_NodeUtils.GetSize( lN );

            numberOfEdges = 1 << subdivisions; // Fast 2^n for integer types.
            numberOfVertices = numberOfEdges + 1;
            edgeLength = size / numberOfEdges; // size represents the edge length of the original square, twice the radius.
            minX = center.x - (size / 2f); // center minus half the edge length of the cube.
            minY = center.y - (size / 2f);

            edgeSubdivisionRelative = new NativeArray<int>( 4, Allocator.TempJob );
            for( int i = 0; i < edgeSubdivisionRelative.Length; i++ )
            {
                if( quad.Edges[i] == null )
                {
                    edgeSubdivisionRelative[i] = 0;
                    continue;
                }
                edgeSubdivisionRelative[i] = quad.Edges[i].SubdivisionLevel - quad.SubdivisionLevel;
            }

            resultVertices = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.TempJob );
            resultNormals = new NativeArray<Vector3>( numberOfVertices * numberOfVertices, Allocator.TempJob );
            resultUvs = new NativeArray<Vector2>( numberOfVertices * numberOfVertices, Allocator.TempJob );
            resultTriangles = new NativeArray<int>( (numberOfEdges * numberOfEdges) * 6, Allocator.TempJob );
        }

        public void Finish( LODQuad quad )
        {
            // Finish is called on the main thread to collect the result and dispose of the job.

            Mesh mesh = new Mesh();

            mesh.SetVertices( resultVertices );
            mesh.SetNormals( resultNormals );
            mesh.SetUVs( 0, resultUvs );
            mesh.SetTriangles( resultTriangles.ToArray(), 0 );
            // tangents calc'd here because job can't create Mesh object to calc them.
            mesh.RecalculateTangents();
            mesh.FixTangents(); // fix broken tangents.
            mesh.RecalculateBounds();

            quad.SetMesh( mesh );

            edgeSubdivisionRelative.Dispose();

            resultVertices.Dispose();
            resultNormals.Dispose();
            resultUvs.Dispose();
            resultTriangles.Dispose();
        }

        public void Execute() // Called by Unity from a job thread.
        {
            GenerateCubeSphereFace();
        }

        int GetIndex( int x, int y )
        {
            return (x * numberOfEdges) + x + y;
        }

        /// <summary>
        /// The method that generates the PQS mesh projected onto a sphere of the specified radius, with its origin at the center of the cube projected onto the same sphere.
        /// </summary>
        /// <param name="lN">How many times this mesh was subdivided (l0, l1, l2, ...).</param>
        public void GenerateCubeSphereFace()
        {
            // The origin of a valid, the center will never be at any of the edges of its ancestors, and will always be at the point where the inner edges of its direct children meet.

            Direction3D face = Direction3DUtils.BasisFromVector( origin.normalized );

            if( numberOfVertices > 65535 )
            {
                // technically wrong, since Mesh.indexFormat can be switched to 32 bit, but i'll leave this for now. Meshes don't have to be over that value anyway because laggy and big and far away.
                throw new ArgumentOutOfRangeException( $"Unity's Mesh can contain at most 65535 vertices (16-bit buffer). Tried to create a Mesh with {numberOfVertices}." );
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
                    resultTriangles[triIndex] = index; // A
                    resultTriangles[triIndex + 1] = index + numberOfVertices + 1; // D
                    resultTriangles[triIndex + 2] = index + numberOfVertices; // C

                    resultTriangles[triIndex + 3] = index; // A
                    resultTriangles[triIndex + 4] = index + 1; // B
                    resultTriangles[triIndex + 5] = index + numberOfVertices + 1; // D

                    triIndex += 6;
                }
            }

            for( int x = 0; x < numberOfVertices; x++ )
            {
                for( int y = 0; y < numberOfVertices; y++ )
                {
                    int index = GetIndex( x, y );

                    float quadX = (x * edgeLength) + minX; // This might need to be turned into a double perhaps (for large bodies with lots of subdivs).
                    float quadY = (y * edgeLength) + minY;

                    Vector3Dbl posD = face.GetSpherePointDbl( quadX, quadY );

                    Vector3 unitSpherePos = (Vector3)posD;

                    Vector3Dbl temporaryHeightOffset_Removelater = posD * Math.Sin( (unitSpherePos.x + unitSpherePos.y + unitSpherePos.z) * radius );

                    resultVertices[index] = (Vector3)(((posD * radius) + temporaryHeightOffset_Removelater) - (Vector3Dbl)origin);

                    const float margin = 0.0f; // margin can be 0 when the texture wrap mode is set to mirror.
                    resultUvs[index] = new Vector2( quadX * (0.5f - margin) + 0.5f, quadY * (0.5f - margin) + 0.5f );

                    // Normals after displacing by heightmap will need to be calculated by hand instead of with RecalculateNormals() to avoid seams not matching up.
                    // normals can be calculated by adding the normals of each face to its vertices, then normalizing.
                    // - this will compute smooth VERTEX normals!!
                    resultNormals[index] = unitSpherePos;
                }
            }


            // the 4 repeated chunnks of code are ugly as fuck.

            if( edgeSubdivisionRelative[0] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[0]);
                int x = 0;
                for( int y = 0; y < numberOfVertices - step; y += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x, y + step );
                    for( int y2 = 0; y2 < step; y2++ )
                    {
                        int index = GetIndex( x, y + y2 );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)y2 / step );
                    }
                }
            }

            if( edgeSubdivisionRelative[1] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[1]);
                int x = numberOfVertices - 1;
                for( int y = 0; y < numberOfVertices - step; y += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x, y + step );
                    for( int y2 = 0; y2 < step; y2++ )
                    {
                        int index = GetIndex( x, y + y2 );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)y2 / step );
                    }
                }
            }

            if( edgeSubdivisionRelative[2] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[2]);
                int y = 0;
                for( int x = 0; x < numberOfVertices - step; x += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x + step, y );
                    for( int x2 = 0; x2 < step; x2++ )
                    {
                        int index = GetIndex( x + x2, y );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)x2 / step );
                    }
                }
            }
            if( edgeSubdivisionRelative[3] < 0 )
            {
                int step = (1 << -edgeSubdivisionRelative[3]);
                int y = numberOfVertices - 1;
                for( int x = 0; x < numberOfVertices - step; x += step )
                {
                    int indexMin = GetIndex( x, y );
                    int indexMax = GetIndex( x + step, y );
                    for( int x2 = 0; x2 < step; x2++ )
                    {
                        int index = GetIndex( x + x2, y );
                        // find index of interval.
                        // smoothly blend.

                        resultVertices[index] = Vector3.Lerp( resultVertices[indexMin], resultVertices[indexMax], (float)x2 / step );
                    }
                }
            }

            // Sadly can't calculate tangents properly easily here.
            // custom method is fucky, and a Mesh object can't be created here.
        }
    }
}