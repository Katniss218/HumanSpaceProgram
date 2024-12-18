using HSP.CelestialBodies.Surfaces;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    public class LODQuadModifier_FinalizeMesh : ILODQuadModifier
    {
        // INFO

        // Running some other modifiers in the same stage as this one may result in race conditions. Specifically modifiers that modify the vertices of the current mesh.

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public ILODQuadJob GetJob()
        {
            return new Job();
        }

        public struct Job : ILODQuadJob
        {
            double radius;
            Vector3Dbl origin;
            Direction3D face;
            float edgeLength;
            Vector2 minCorner;

            int totalVertices;

            int sideVertices;
            int sideEdges;

            [ReadOnly]
            NativeArray<int> resultTriangles;
            [ReadOnly]
            NativeArray<Vector2> resultUVs;
            [ReadOnly]
            NativeArray<Vector3Dbl> resultVertices;
            NativeArray<Vector3> meshVertices;

            NativeArray<Vector3> resultNormals;
            NativeArray<Vector4> resultTangents;

            int stepXn;
            int stepXp;
            int stepYn;
            int stepYp;

            [ReadOnly]
            NativeArray<Vector3Dbl> resultVerticesXn;
            float edgeLengthXn;
            Vector2 minCornerXn;

            [ReadOnly]
            NativeArray<Vector3Dbl> resultVerticesXp;
            float edgeLengthXp;
            Vector2 minCornerXp;

            [ReadOnly]
            NativeArray<Vector3Dbl> resultVerticesYn;
            float edgeLengthYn;
            Vector2 minCornerYn;

            [ReadOnly]
            NativeArray<Vector3Dbl> resultVerticesYp;
            float edgeLengthYp;
            Vector2 minCornerYp;

            public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData rAdditional )
            {
                radius = (float)r.CelestialBody.Radius;
                origin = r.Node.SphereCenter * radius;
                stepXn = 1 << (r.Node.SubdivisionLevel - r.Node.Xn.SubdivisionLevel);
                stepXp = 1 << (r.Node.SubdivisionLevel - r.Node.Xp.SubdivisionLevel);
                stepYn = 1 << (r.Node.SubdivisionLevel - r.Node.Yn.SubdivisionLevel);
                stepYp = 1 << (r.Node.SubdivisionLevel - r.Node.Yp.SubdivisionLevel);

                sideEdges = r.SideEdges;
                sideVertices = r.SideVertices;
                totalVertices = sideVertices * sideVertices;

                edgeLength = r.Node.Size / sideEdges;
                float halfSize = r.Node.Size / 2;
                minCorner = r.Node.FaceCenter - (Vector2.one * halfSize);
                face = r.Node.Face;

                resultTriangles = r.ResultTriangles;
                resultVertices = r.ResultVertices;
                resultUVs = r.ResultUVs;
                resultNormals = r.ResultNormals;
                resultTangents = new NativeArray<Vector4>( totalVertices, Allocator.Persistent );
                meshVertices = new NativeArray<Vector3>( sideVertices * sideVertices, Allocator.Persistent );

                var xn = rAdditional.allQuads[r.Node.Xn];
                float halfSizeXn = r.Node.Xn.Size / 2;
                minCornerXn = r.Node.Xn.FaceCenter - (Vector2.one * halfSizeXn);
                edgeLengthXn = r.Node.Xn.Size / sideEdges;
                resultVerticesXn = xn.HasNew ? xn.newR.ResultVertices : xn.oldR.ResultVertices;

                var xp = rAdditional.allQuads[r.Node.Xp];
                float halfSizeXp = r.Node.Xp.Size / 2;
                minCornerXp = r.Node.Xp.FaceCenter - (Vector2.one * halfSizeXp);
                edgeLengthXp = r.Node.Xp.Size / sideEdges;
                resultVerticesXp = xp.HasNew ? xp.newR.ResultVertices : xp.oldR.ResultVertices;

                var yn = rAdditional.allQuads[r.Node.Yn];
                float halfSizeYn = r.Node.Yn.Size / 2;
                minCornerYn = r.Node.Yn.FaceCenter - (Vector2.one * halfSizeYn);
                edgeLengthYn = r.Node.Yn.Size / sideEdges;
                resultVerticesYn = yn.HasNew ? yn.newR.ResultVertices : yn.oldR.ResultVertices;

                var yp = rAdditional.allQuads[r.Node.Yp];
                float halfSizeYp = r.Node.Yp.Size / 2;
                minCornerYp = r.Node.Yp.FaceCenter - (Vector2.one * halfSizeYp);
                edgeLengthYp = r.Node.Yp.Size / sideEdges;
                resultVerticesYp = yp.HasNew ? yp.newR.ResultVertices : yp.oldR.ResultVertices;
            }

            public void Finish( LODQuadRebuildData r )
            {
                r.ResultMesh.SetVertices( meshVertices );
                r.ResultMesh.SetNormals( resultNormals );
                r.ResultMesh.SetUVs( 0, resultUVs );
                r.ResultMesh.SetTriangles( resultTriangles.ToArray(), 0 );

                r.ResultMesh.SetNormals( resultNormals );
                r.ResultMesh.SetTangents( resultTangents );
                r.ResultMesh.RecalculateBounds();
            }

            public void Dispose()
            {
                resultTangents.Dispose();
                meshVertices.Dispose();
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private int GetIndex( int x, int y )
            {
                return (x * sideEdges) + x + y;
            }

            /// <summary>
            /// Transforms from overflowed coordinates in the space of 1 face into the other face.
            /// </summary>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public static (float x, float y) FixCoordinates( Direction3D face, float x, float y )
            {
                switch( face )
                {
                    case Direction3D.Xn:
                        if( x < -1 )
                        {
                            return (x + 2, y);
                        }
                        else if( x > 1 )
                        {
                            return (x - 2, y);
                        }
                        else if( y < -1 )
                        {
                            return (x, y + 2);
                        }
                        else if( y > 1 )
                        {
                            return (x, y - 2);
                        }
                        break;

                    case Direction3D.Xp:
                        if( x < -1 )
                        {
                            return (x + 2, y);
                        }
                        else if( x > 1 )
                        {
                            return (x - 2, y);
                        }
                        else if( y < -1 )
                        {
                            return (-x, -(y + 2));
                        }
                        else if( y > 1 )
                        {
                            return (-x, -(y - 2));
                        }
                        break;

                    case Direction3D.Yn:
                    {
                        if( x < -1 )
                        {
                            return (x + 2, y);
                        }
                        else if( x > 1 )
                        {
                            return (x - 2, y);
                        }
                        else if( y < -1 )
                        {
                            return (-(y + 2), x);
                        }
                        else if( y > 1 )
                        {
                            return (y - 2, -x);
                        }

                        break;
                    }

                    case Direction3D.Yp:
                    {
                        if( x < -1 )
                        {
                            return (x + 2, y);
                        }
                        else if( x > 1 )
                        {
                            return (x - 2, y);
                        }
                        else if( y < -1 )
                        {
                            return (y + 2, -x);
                        }
                        else if( y > 1 )
                        {
                            return (-(y - 2), x);
                        }

                        break;
                    }

                    case Direction3D.Zn:
                    {
                        if( x < -1 )
                        {
                            return (y, -(x + 2));
                        }
                        else if( x > 1 )
                        {
                            return (-y, x - 2);
                        }
                        else if( y < -1 )
                        {
                            return (-x, -(y + 2));
                        }
                        else if( y > 1 )
                        {
                            return (x, y - 2);
                        }

                        break;
                    }

                    case Direction3D.Zp:
                    {
                        if( x < -1 )
                        {
                            return (-y, x + 2);
                        }
                        else if( x > 1 )
                        {
                            return (y, -(x - 2));
                        }
                        else if( y < -1 )
                        {
                            return (x, y + 2);
                        }
                        else if( y > 1 )
                        {
                            return (-x, -(y - 2));
                        }

                        break;
                    }
                }
                return (x, y);
            }

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private (Vector3Dbl self, Vector3Dbl xn, Vector3Dbl xp, Vector3Dbl yn, Vector3Dbl yp) GetVertex( int x, int y )
            {
                Vector3Dbl self = resultVertices[GetIndex( x, y )];
                Vector3Dbl xn, xp, yn, yp;
                if( x == 0 )
                {
                    float xnx = (x * edgeLength) + minCorner.x - edgeLengthXn;
                    float xny = (y * edgeLength) + minCorner.y;
                    (xnx, xny) = FixCoordinates( face, xnx, xny );
                    int indexXnX = (int)((xnx - minCornerXn.x) / edgeLengthXn);
                    int indexXnY = (int)((xny - minCornerXn.y) / edgeLengthXn);
                    xn = resultVerticesXn[GetIndex( indexXnX, indexXnY )];
                    xp = resultVertices[GetIndex( x + 1 * stepXn, y )]; // Multiplied by step size to take the same step into both quads.
                }
                else if( x == sideVertices - 1 )
                {
                    float xpx = (x * edgeLength) + minCorner.x + edgeLengthXp;
                    float xpy = (y * edgeLength) + minCorner.y;
                    (xpx, xpy) = FixCoordinates( face, xpx, xpy );
                    int indexXpX = (int)((xpx - minCornerXp.x) / edgeLengthXp);
                    int indexXpY = (int)((xpy - minCornerXp.y) / edgeLengthXp);
                    xn = resultVertices[GetIndex( x - 1 * stepXp, y )];
                    xp = resultVerticesXp[GetIndex( indexXpX, indexXpY )];
                }
                else
                {
                    xn = resultVertices[GetIndex( x - 1, y )];
                    xp = resultVertices[GetIndex( x + 1, y )];
                }

                if( y == 0 )
                {
                    float ynx = (x * edgeLength) + minCorner.x;
                    float yny = (y * edgeLength) + minCorner.y - edgeLengthYn;
                    (ynx, yny) = FixCoordinates( face, ynx, yny );
                    int indexYnX = (int)((ynx - minCornerYn.x) / edgeLengthYn);
                    int indexYnY = (int)((yny - minCornerYn.y) / edgeLengthYn);
                    yn = resultVerticesYn[GetIndex( indexYnX, indexYnY )];
                    yp = resultVertices[GetIndex( x, y + stepYn )];
                }
                else if( y == sideVertices - 1 )
                {
                    float ypx = (x * edgeLength) + minCorner.x;
                    float ypy = (y * edgeLength) + minCorner.y + edgeLengthYp;
                    (ypx, ypy) = FixCoordinates( face, ypx, ypy );
                    int indexYpX = (int)((ypx - minCornerYp.x) / edgeLengthYp);
                    int indexYpY = (int)((ypy - minCornerYp.y) / edgeLengthYp);
                    yn = resultVertices[GetIndex( x, y - stepYp )];
                    yp = resultVerticesYp[GetIndex( indexYpX, indexYpY )];
                }
                else
                {
                    yn = resultVertices[GetIndex( x, y - 1 )];
                    yp = resultVertices[GetIndex( x, y + 1 )];
                }

                return (self, xn, xp, yn, yp);
            }

            public void Execute()
            {
                for( int x = 0; x < sideVertices; x++ )
                {
                    for( int y = 0; y < sideVertices; y++ )
                    {
                        int index = GetIndex( x, y );
                        var (_, xn, xp, yn, yp) = GetVertex( x, y );

                        Vector3 tangent = (xn - xp).NormalizeToVector3();
                        Vector3 bitangent = (yn - yp).NormalizeToVector3();

                        meshVertices[index] = (Vector3)(resultVertices[index] - origin);
                        resultNormals[index] = Vector3.Cross( bitangent, tangent );
                        resultTangents[index] = new Vector4( tangent.x, tangent.y, tangent.z, 1 );
                    }
                }

                // below is lerping neighboring larger quads. should only operate on local arrays.

                if( stepXn != 0 )
                {
                    int x = 0;
                    for( int y = 0; y < sideVertices - stepXn; y += stepXn )
                    {
                        int indexMin = GetIndex( x, y );
                        int indexMax = GetIndex( x, y + stepXn );
                        for( int y2 = 0; y2 < stepXn; y2++ )
                        {
                            int index = GetIndex( x, y + y2 );

                            meshVertices[index] = (Vector3)Vector3Dbl.Lerp( meshVertices[indexMin], meshVertices[indexMax], (float)y2 / stepXn );
                            resultNormals[index] = Vector3.Lerp( resultNormals[indexMin], resultNormals[indexMax], (float)y2 / stepXn ).normalized;
                            resultTangents[index] = Vector4.Lerp( resultTangents[indexMin], resultTangents[indexMax], (float)y2 / stepXn );
                        }
                    }
                }

                if( stepXp != 0 )
                {
                    int x = sideVertices - 1;
                    for( int y = 0; y < sideVertices - stepXp; y += stepXp )
                    {
                        int indexMin = GetIndex( x, y );
                        int indexMax = GetIndex( x, y + stepXp );
                        for( int y2 = 0; y2 < stepXp; y2++ )
                        {
                            int index = GetIndex( x, y + y2 );

                            meshVertices[index] = (Vector3)Vector3Dbl.Lerp( meshVertices[indexMin], meshVertices[indexMax], (float)y2 / stepXp );
                            resultNormals[index] = Vector3.Lerp( resultNormals[indexMin], resultNormals[indexMax], (float)y2 / stepXp ).normalized;
                            resultTangents[index] = Vector4.Lerp( resultTangents[indexMin], resultTangents[indexMax], (float)y2 / stepXp );
                        }
                    }
                }

                if( stepYn != 0 )
                {
                    int y = 0;
                    for( int x = 0; x < sideVertices - stepYn; x += stepYn )
                    {
                        int indexMin = GetIndex( x, y );
                        int indexMax = GetIndex( x + stepYn, y );
                        for( int x2 = 0; x2 < stepYn; x2++ )
                        {
                            int index = GetIndex( x + x2, y );

                            meshVertices[index] = (Vector3)Vector3Dbl.Lerp( meshVertices[indexMin], meshVertices[indexMax], (float)x2 / stepYn );
                            resultNormals[index] = Vector3.Lerp( resultNormals[indexMin], resultNormals[indexMax], (float)x2 / stepYn ).normalized;
                            resultTangents[index] = Vector4.Lerp( resultTangents[indexMin], resultTangents[indexMax], (float)x2 / stepYn );
                        }
                    }
                }

                if( stepYp != 0 )
                {
                    int y = sideVertices - 1;
                    for( int x = 0; x < sideVertices - stepYp; x += stepYp )
                    {
                        int indexMin = GetIndex( x, y );
                        int indexMax = GetIndex( x + stepYp, y );
                        for( int x2 = 0; x2 < stepYp; x2++ )
                        {
                            int index = GetIndex( x + x2, y );

                            meshVertices[index] = (Vector3)Vector3Dbl.Lerp( meshVertices[indexMin], meshVertices[indexMax], (float)x2 / stepYp );
                            resultNormals[index] = Vector3.Lerp( resultNormals[indexMin], resultNormals[indexMax], (float)x2 / stepYp ).normalized;
                            resultTangents[index] = Vector4.Lerp( resultTangents[indexMin], resultTangents[indexMax], (float)x2 / stepYp );
                        }
                    }
                }
            }
        }
    }
}