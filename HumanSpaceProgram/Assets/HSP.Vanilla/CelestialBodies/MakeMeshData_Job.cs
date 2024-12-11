using HSP.CelestialBodies.Surfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace HSP.Vanilla.CelestialBodies
{


    public struct MakeMeshData_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;
        Direction3D face;
        float quadSize;
        Vector2 faceCenter;

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
        float quadSizeXn;
        Vector2 faceCenterXn;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesXp;
        float quadSizeXp;
        Vector2 faceCenterXp;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesYn;
        float quadSizeYn;
        Vector2 faceCenterYn;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesYp;
        float quadSizeYp;
        Vector2 faceCenterYp;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData rAdditional )
        {
            radius = (float)r.CelestialBody.Radius;
            origin = r.Node.SphereCenter * radius;
            quadSize = r.Node.Size;
            faceCenter = r.Node.FaceCenter;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            totalVertices = sideVertices * sideVertices;

            resultTriangles = r.ResultTriangles;
            resultVertices = r.ResultVertices;
            resultUVs = r.ResultUVs;
            resultNormals = r.ResultNormals;
            resultTangents = new NativeArray<Vector4>( totalVertices, Allocator.TempJob );

            var xn = rAdditional.allQuads[r.Node.Xn];
            faceCenterXn = r.Node.Xn.FaceCenter;
            quadSizeXn = r.Node.Xn.Size;
            if( xn.hasNew )
            {
                resultVerticesXn = xn.@new.ResultVertices;
            }
            else
            {
#error TODO - Keep these buffers until the quad made with them is destroyed.
                xn.old.GetVertices( resultVerticesXn );
            }

            var xp = rAdditional.allQuads[r.Node.Xp];
            faceCenterXp = r.Node.Xp.FaceCenter;
            quadSizeXp = r.Node.Xp.Size;
            if( xp.hasNew )
            {
                resultVerticesXp = xp.@new.ResultVertices;
            }
            else
            {
                // this array seems to need to be allocated ahead.
                xp.old.GetVertices( resultVerticesXp );
            }

            var yn = rAdditional.allQuads[r.Node.Yn];
            faceCenterYn = r.Node.Yn.FaceCenter;
            quadSizeYn = r.Node.Yn.Size;
            if( yn.hasNew )
            {
                resultVerticesYn = yn.@new.ResultVertices;
            }
            else
            {
                yn.old.GetVertices( resultVerticesYn );
            }

            var yp = rAdditional.allQuads[r.Node.Yp];
            faceCenterYp = r.Node.Yp.FaceCenter;
            quadSizeYp = r.Node.Yp.Size;
            if( yp.hasNew )
            {
                resultVerticesYp = yp.@new.ResultVertices;
            }
            else
            {
                yp.old.GetVertices( resultVerticesYp );
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

#warning TODO - only dispose of the new arrays (but if I keep the original arrays in the quads, that won't be necessary, they'd be disposed after.
            //resultVerticesXn.Dispose();
            //resultVerticesXp.Dispose();
            //resultVerticesYn.Dispose();
            //resultVerticesYp.Dispose();
        }

        public ILODQuadJob Clone()
        {
            return new MakeMeshData_Job();
        }

        int GetIndex( int x, int y )
        {
            return (x * sideEdges) + x + y;
        }

        /// <summary>
        /// Transforms from overflowed coordinates in the space of 1 face into the other face.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void FixCoordinates( ref Direction3D face, ref double x, ref double y )
        {
            // uv = fix(uv * 2.0 - 1.0) * 0.5 + 0.5
            // xy = fix(xy)

            if( face == Direction3D.Xn )
            {
                if( x < -1 )
                {
                    face = Direction3D.Yn;
                    x += 2;
                }
                else if( x > 1 )
                {
                    face = Direction3D.Yp;
                    x -= 2;
                }
                else if( y < -1 )
                {
                    face = Direction3D.Zn;
                    y += 2;
                }
                else if( y > 1 )
                {
                    face = Direction3D.Zp;
                    y -= 2;
                }
            }

            if( face == Direction3D.Xp )
            {
                if( x < -1 )
                {
                    face = Direction3D.Yp;
                    x += 2;
                }
                else if( x > 1 )
                {
                    face = Direction3D.Yn;
                    x -= 2;
                }
                else if( y < -1 )
                {
                    face = Direction3D.Zn;
                    x = -x;
                    y = -(y + 2);
                }
                else if( y > 1 )
                {
                    face = Direction3D.Zp;
                    x = -x;
                    y = -(y - 2);
                }
            }

            if( face == Direction3D.Yn )
            {
                if( x < -1 )
                {
                    face = Direction3D.Xp;
                    x += 2;
                }
                else if( x > 1 )
                {
                    face = Direction3D.Xn;
                    x -= 2;
                }
                else if( y < -1 )
                {
                    face = Direction3D.Zn;
                    var tempy = x;
                    x = -(y + 2);
                    y = tempy;
                }
                else if( y > 1 )
                {
                    face = Direction3D.Zp;
                    var tempy = -x;
                    x = y - 2;
                    y = tempy;
                }
            }

            if( face == Direction3D.Yp )
            {
                if( x < -1 )
                {
                    face = Direction3D.Xn;
                    x += 2;
                }
                else if( x > 1 )
                {
                    face = Direction3D.Xp;
                    x -= 2;
                }
                else if( y < -1 )
                {
                    face = Direction3D.Zn;
                    var tempy = -x;
                    x = y + 2;
                    y = tempy;
                }
                else if( y > 1 )
                {
                    face = Direction3D.Zp;
                    var tempy = x;
                    x = -(y - 2);
                    y = tempy;
                }
            }

            if( face == Direction3D.Zn )
            {
                if( x < -1 )
                {
                    face = Direction3D.Yn;
                    var tempy = -(x + 2);
                    x = y;
                    y = tempy;
                }
                else if( x > 1 )
                {
                    face = Direction3D.Yp;
                    var tempy = (x - 2);
                    x = y;
                    y = tempy;
                }
                else if( y < -1 )
                {
                    face = Direction3D.Xp;
                    var tempy = -(y + 2);
                    x = -x;
                    y = tempy;
                }
                else if( y > 1 )
                {
                    face = Direction3D.Xn;
                    y = y - 2;
                }
            }

            if( face == Direction3D.Zp )
            {
                if( x < -1 )
                {
                    face = Direction3D.Yn;
                    var tempy = (x + 2);
                    x = -y;
                    y = tempy;
                }
                else if( x > 1 )
                {
                    face = Direction3D.Yp;
                    var tempy = -(x - 2);
                    x = y;
                    y = tempy;
                }
                else if( y < -1 )
                {
                    face = Direction3D.Xn;
                    y = y + 2;
                }
                else if( y > 1 )
                {
                    face = Direction3D.Xp;
                    var tempy = -(y - 2);
                    x = -x;
                    y = tempy;
                }
            }
        }

        private (Vector3 self, Vector3 xn, Vector3 xp, Vector3 yn, Vector3 yp) GetVertex( int x, int y )
        {
            Vector3 self = resultVertices[GetIndex( x, y )];
            Vector3 xn, xp, yn, yp;

            Direction3D _ = default;
            if( x == 0 )
            {
                double xnx = ((x - 1) * quadSize * 2 - 1) + faceCenter.x;
                double xny = (y * quadSize * 2 - 1) + faceCenter.y;
                FixCoordinates( ref _, ref xnx, ref xny );
                int indexXnX = (int)((xnx - faceCenterXn.x) / quadSizeXn * 0.5 + 0.5);
                int indexXnY = (int)((xny - faceCenterXn.y) / quadSizeXn * 0.5 + 0.5);

                xn = resultVerticesXn[GetIndex( indexXnX, indexXnY )];
                xp = resultVerticesXn[GetIndex( x + 1, y )];
            }
            else if( x == sideEdges - 1 )
            {
                double xpx = ((x + 1) * quadSize * 2 - 1) + faceCenter.x;
                double xpy = (y * quadSize * 2 - 1) + faceCenter.y;
                FixCoordinates( ref _, ref xpx, ref xpy );
                int indexXpX = (int)((xpx - faceCenterXp.x) / quadSizeXp * 0.5 + 0.5);
                int indexXpY = (int)((xpy - faceCenterXp.y) / quadSizeXp * 0.5 + 0.5);

                xn = resultVerticesXn[GetIndex( x - 1, y )];
                xp = resultVerticesXn[GetIndex( indexXpX, indexXpY )];
            }
            else
            {
                xn = resultVerticesXn[GetIndex( x - 1, y )];
                xp = resultVerticesXn[GetIndex( x + 1, y )];
            }

            if( y == 0 )
            {
                double ynx = (x * quadSize * 2 - 1) + faceCenter.x;
                double yny = ((y - 1) * quadSize * 2 - 1) + faceCenter.y;
                FixCoordinates( ref _, ref ynx, ref yny );
                int indexYnX = (int)((ynx - faceCenterYn.x) / quadSizeYn * 0.5 + 0.5);
                int indexYnY = (int)((yny - faceCenterYn.y) / quadSizeYn * 0.5 + 0.5);

                yn = resultVerticesXn[GetIndex( indexYnX, indexYnY )];
                yp = resultVerticesXn[GetIndex( x, y + 1 )];
            }
            else if( y == sideEdges - 1 )
            {
                double ypx = (x * quadSize * 2 - 1) + faceCenter.x;
                double ypy = ((y + 1) * quadSize * 2 - 1) + faceCenter.y;
                FixCoordinates( ref _, ref ypx, ref ypy );
                int indexYpX = (int)((ypx - faceCenterYp.x) / quadSizeYp * 0.5 + 0.5);
                int indexYpY = (int)((ypy - faceCenterYp.y) / quadSizeYp * 0.5 + 0.5);

                yn = resultVerticesXn[GetIndex( x, y - 1 )];
                yp = resultVerticesXn[GetIndex( indexYpX, indexYpY )];
            }
            else
            {
                yn = resultVerticesXn[GetIndex( x, y - 1 )];
                yp = resultVerticesXn[GetIndex( x, y + 1 )];
            }

            return (self, xn, xp, yn, yp);
        }

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