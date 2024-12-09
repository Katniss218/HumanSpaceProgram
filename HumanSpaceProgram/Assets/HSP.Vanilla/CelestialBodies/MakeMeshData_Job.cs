using HSP.CelestialBodies.Surfaces;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public struct MakeMeshData_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;

        int totalVertices;

        int sideVertices;
        int sideEdges;

        [ReadOnly]
        NativeArray<int> resultTriangles;
        [ReadOnly]
        NativeArray<Vector2> resultUVs;
        [ReadOnly]
        NativeArray<Vector3> resultVertices;

        NativeArray<Vector3> resultNormals;
        NativeArray<Vector4> resultTangents;

#warning TODO - Some mechanism for safety regarding when you can schedule these would be nice, as these arrays can be writeable, so no other jobs should be writing to them.
        // Also some documentation would be nice.

        // Using these without readonly can cause exceptions being thrown,
        // writing to an array when another job reads from it can also cause big synchronization problems
        //   (this is the reason that we can't e.g. use other normals when setting our normals).

        // This essentially boils down to that using certain 'job' types in the same stage causes race conditions (depending on what these jobs do).

        [ReadOnly]
        NativeArray<Vector3> resultVerticesXn;
        bool availableXn;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesXp;
        bool availableXp;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesYn;
        bool availableYn;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesYp;
        bool availableYp;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, IReadOnlyDictionary<LODQuadTreeNode, LODQuadRebuildData> rAll )
        {
            radius = (float)r.CelestialBody.Radius;
            origin = r.Node.SphereCenter * radius;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            totalVertices = sideVertices * sideVertices;

            resultTriangles = r.ResultTriangles;
            resultVertices = r.ResultVertices;
            resultUVs = r.ResultUVs;
            resultNormals = r.ResultNormals;
            resultTangents = new NativeArray<Vector4>( totalVertices, Allocator.TempJob );

            if( rAll.TryGetValue( r.Node.Xp, out var neighbor ) )
            {
                resultVerticesXn = neighbor.ResultVertices;
                availableXn = true;
            }
            else
            {
                resultVerticesXn = resultVertices;
            }
            if( rAll.TryGetValue( r.Node.Xp, out neighbor ) )
            {
                resultVerticesXp = neighbor.ResultVertices;
                availableXp = true;
            }
            else
            {
                resultVerticesXp = resultVertices;
            }
            if( rAll.TryGetValue( r.Node.Yn, out neighbor ) )
            {
                resultVerticesYn = neighbor.ResultVertices;
                availableYn = true;
            }
            else
            {
                resultVerticesYn = resultVertices;
            }
            if( rAll.TryGetValue( r.Node.Yp, out neighbor ) )
            {
                resultVerticesYp = neighbor.ResultVertices;
                availableYp = true;
            }
            else
            {
                resultVerticesYp = resultVertices;
            }
        }

        public void Finish( LODQuadRebuildData r )
        {
            r.Mesh.SetNormals( resultNormals );
            r.Mesh.SetTangents( resultTangents );
            r.Mesh.RecalculateBounds();
        }

        public void Dispose()
        {
            resultTangents.Dispose();
        }

        public ILODQuadJob Clone()
        {
            return new MakeMeshData_Job();
        }

        int GetIndex( int x, int y )
        {
            return (x * sideEdges) + x + y;
        }

#warning TODO - get vertex to access the appropriate neighbor, alongside the reference to said neighbor.

        public void Execute()
        {
            for( int x = 0; x < sideEdges; x++ )
            {
                for( int y = 0; y < sideEdges; y++ )
                {
                    int index = GetIndex( x, y );

                    int indexXn = GetIndex( x - 1, y );
                    int indexXp = GetIndex( x + 1, y );
                    int indexYn = GetIndex( x, y - 1 );
                    int indexYp = GetIndex( x, y + 1 );

                    if( indexXn < 0 )
                    {
                        // sample from quad Xn
                    }
                    else if( indexXp > totalVertices )
                    {
                        // sample from quad Xp
                    }
                    if( indexYn < 0 )
                    {
                        // sample from quad Yn
                    }
                    else if( indexYp > totalVertices )
                    {
                        // sample from quad Yp
                    }

                    int index1 = indexXn;
                    int index2 = indexYn;
                    //bool flippedX = false;
                    //bool flippedY = false;

                    if( x == 0 )
                    {
                        index1 = indexXp;
                        //flippedX = true;
                    }
                    if( y == 0 )
                    {
                        index2 = indexYp;
                        //flippedY = true;
                    }

                    Vector3 tangent = (resultVertices[index] - resultVertices[index1]).normalized;
                    Vector3 bitangent = (resultVertices[index] - resultVertices[index2]).normalized;
                    Vector3 normal = Vector3.Cross( bitangent, tangent );
                    if( (x == 0) ^ (y == 0) )
                    {
                        normal = -normal;
                    }

                    resultNormals[index] = normal;
                }
            }

            int triangleCount = (sideEdges * sideEdges) * 6;

            Vector3[] tan1 = new Vector3[totalVertices];
            Vector3[] tan2 = new Vector3[totalVertices];
            for( int a = 0; a < triangleCount; a += 3 )
            {
                int i1 = resultTriangles[a + 0];
                int i2 = resultTriangles[a + 1];
                int i3 = resultTriangles[a + 2];

                Vector3 v1 = resultVertices[i1];
                Vector3 v2 = resultVertices[i2];
                Vector3 v3 = resultVertices[i3];

                Vector2 w1 = resultUVs[i1];
                Vector2 w2 = resultUVs[i2];
                Vector2 w3 = resultUVs[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);

                Vector3 sdir = new Vector3( (t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r );
                Vector3 tdir = new Vector3( (s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r );

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for( int a = 0; a < totalVertices; ++a )
            {
                Vector3 normal = resultNormals[a];
                Vector3 t = tan1[a];

                Vector3 tmp = (t - normal * Vector3.Dot( normal, t )).normalized;
                Vector4 tangent = new Vector4( tmp.x, tmp.y, tmp.z );
                tangent.w = (Vector3.Dot( Vector3.Cross( normal, t ), tan2[a] ) < 0.0f) ? -1.0f : 1.0f;

                resultTangents[a] = tangent;
            }
        }
    }
}