using HSP.CelestialBodies.Surfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.UI.Image;

namespace HSP.Vanilla.CelestialBodies
{


    public struct MakeMeshData_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;
        Direction3D face;
        float edgeLength;
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

        float halfSize;
        float halfSizeXn;
        float halfSizeXp;
        float halfSizeYn;
        float halfSizeYp;

        Vector3Dbl originXn;
        Vector3Dbl originXp;
        Vector3Dbl originYn;
        Vector3Dbl originYp;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesXn;
        float edgeLengthXn;
        Vector2 faceCenterXn;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesXp;
        float edgeLengthXp;
        Vector2 faceCenterXp;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesYn;
        float edgeLengthYn;
        Vector2 faceCenterYn;

        [ReadOnly]
        NativeArray<Vector3> resultVerticesYp;
        float edgeLengthYp;
        Vector2 faceCenterYp;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r, LODQuadRebuildAdditionalData rAdditional )
        {
            radius = (float)r.CelestialBody.Radius;
            origin = r.Node.SphereCenter * radius;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            totalVertices = sideVertices * sideVertices;

            edgeLength = r.Node.Size / sideEdges;
            faceCenter = r.Node.FaceCenter;// - (Vector2.one * (r.Node.Size / 2));
            halfSize = r.Node.Size / 2;
            face = r.Node.Face;

            resultTriangles = r.ResultTriangles;
            resultVertices = r.ResultVertices;
            resultUVs = r.ResultUVs;
            resultNormals = r.ResultNormals;
            resultTangents = new NativeArray<Vector4>( totalVertices, Allocator.TempJob );

            var xn = rAdditional.allQuads[r.Node.Xn];
            faceCenterXn = r.Node.Xn.FaceCenter;// - (Vector2.one * (r.Node.Xn.Size / 2));
            originXn = r.Node.Xn.SphereCenter;
            halfSizeXn = r.Node.Xn.Size / 2;
            edgeLengthXn = r.Node.Xn.Size / sideEdges;
            resultVerticesXn = xn.HasNew ? xn.newR.ResultVertices : xn.oldR.ResultVertices;

            var xp = rAdditional.allQuads[r.Node.Xp];
            faceCenterXp = r.Node.Xp.FaceCenter;// - (Vector2.one * (r.Node.Xp.Size / 2));
            originXp = r.Node.Xp.SphereCenter;
            halfSizeXp = r.Node.Xp.Size / 2;
            edgeLengthXp = r.Node.Xp.Size / sideEdges;
            resultVerticesXp = xp.HasNew ? xp.newR.ResultVertices : xp.oldR.ResultVertices;

            var yn = rAdditional.allQuads[r.Node.Yn];
            faceCenterYn = r.Node.Yn.FaceCenter;// - (Vector2.one * (r.Node.Yn.Size / 2));
            originYn = r.Node.Yn.SphereCenter;
            halfSizeYn = r.Node.Yn.Size / 2;
            edgeLengthYn = r.Node.Yn.Size / sideEdges;
            resultVerticesYn = yn.HasNew ? yn.newR.ResultVertices : yn.oldR.ResultVertices;

            var yp = rAdditional.allQuads[r.Node.Yp];
            faceCenterYp = r.Node.Yp.FaceCenter;// - (Vector2.one * (r.Node.Yp.Size / 2));
            originYp = r.Node.Yp.SphereCenter;
            halfSizeYp = r.Node.Yp.Size / 2;
            edgeLengthYp = r.Node.Yp.Size / sideEdges;
            resultVerticesYp = yp.HasNew ? yp.newR.ResultVertices : yp.oldR.ResultVertices;
        }

        public void Finish( LODQuadRebuildData r )
        {
            r.ResultMesh.SetNormals( resultNormals );
            r.ResultMesh.SetTangents( resultTangents );
            r.ResultMesh.RecalculateBounds();
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

            else if( face == Direction3D.Xp )
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

            else if( face == Direction3D.Yn )
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

            else if( face == Direction3D.Yp )
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

            else if( face == Direction3D.Zn )
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
                    x = -y;
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

            else if( face == Direction3D.Zp )
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

            Direction3D _;
            if( x == 0 )
            {
                double xnx = ((x - 1) * edgeLength) + faceCenter.x - halfSize;
                double xny = (y * edgeLength) + faceCenter.y - halfSize;
                double xnxB = xnx;
                double xnyB = xny;
                _ = face;
                FixCoordinates( ref _, ref xnx, ref xny );
                int indexXnX = (int)((xnx - faceCenterXn.x + halfSizeXn) / edgeLengthXn);
                int indexXnY = (int)((xny - faceCenterXn.y + halfSizeXn) / edgeLengthXn);
                if( indexXnX < 0 || indexXnY < 0 )
                {
                    Debug.LogError( face + " : " + x + " " + y + " " + xnxB + " : " + xnyB + " : " + faceCenter + " @@@@@@@ " + _ + " : " + indexXnX + " : " + indexXnY + " : " + xnx + " : " + xny + " : " + faceCenterXn );
                }

                xn = resultVerticesXn[GetIndex( indexXnX, indexXnY )];
                xp = resultVertices[GetIndex( x + 1, y )];
            }
            else if( x == sideVertices - 1 )
            {
                double xpx = ((x + 1) * edgeLength) + faceCenter.x - halfSize;
                double xpy = (y * edgeLength) + faceCenter.y - halfSize;
                double xpxB = xpx;
                double xpyB = xpy;
                _ = face;
                FixCoordinates( ref _, ref xpx, ref xpy );
                int indexXpX = (int)((xpx - faceCenterXp.x + halfSizeXp) / edgeLengthXp);
                int indexXpY = (int)((xpy - faceCenterXp.y + halfSizeXp) / edgeLengthXp);

                if( indexXpX < 0 || indexXpY < 0 )
                {
                    Debug.LogError( face + " : " + x + " " + y + " " + xpxB + " : " + xpyB + " : " + faceCenter + " @@@@@@@ " + _ + " : " + indexXpX + " : " + indexXpY + " : " + xpx + " : " + xpy + " : " + faceCenterXp );
                }
                xn = resultVertices[GetIndex( x - 1, y )];
                xp = resultVerticesXp[GetIndex( indexXpX, indexXpY )];
            }
            else
            {
                xn = resultVertices[GetIndex( x - 1, y )];
                xp = resultVertices[GetIndex( x + 1, y )];
            }
            if( y == 0 )
            {
                if( face == Direction3D.Xn && faceCenter == new Vector2( 0.5f, -0.5f ) && x == 32 && y == 0 )
                {

                }
                double ynx = (x * edgeLength) + faceCenter.x - halfSize;
                double yny = ((y - 1) * edgeLength) + faceCenter.y - halfSize;
                double ynxB = ynx;
                double ynyB = yny;
                _ = face;
                FixCoordinates( ref _, ref ynx, ref yny );
                int indexYnX = (int)((ynx - faceCenterYn.x + halfSizeYn) / edgeLengthYn);
                int indexYnY = (int)((yny - faceCenterYn.y + halfSizeYn) / edgeLengthYn);

                if( indexYnX < 0 || indexYnY < 0 )
                {
                    Debug.LogError( face + " : " + x + " " + y + " " + ynxB + " : " + ynyB + " : " + faceCenter + " @@@@@@@ " + _ + " : " + indexYnX + " : " + indexYnY + " : " + ynx + " : " + yny + " : " + faceCenterYn );
                }
                yn = resultVerticesYn[GetIndex( indexYnX, indexYnY )];
                yp = resultVertices[GetIndex( x, y + 1 )];
            }
            else if( y == sideVertices - 1 )
            {
                double ypx = (x * edgeLength) + faceCenter.x - halfSize;
                double ypy = ((y + 1) * edgeLength) + faceCenter.y - halfSize;
                double ypxB = ypx;
                double ypyB = ypy;
                _ = face;
                FixCoordinates( ref _, ref ypx, ref ypy );
                int indexYpX = (int)((ypx - faceCenterYp.x + halfSizeYp) / edgeLengthYp);
                int indexYpY = (int)((ypy - faceCenterYp.y + halfSizeYp) / edgeLengthYp);

                if( indexYpX < 0 || indexYpY < 0 )
                {
                    Debug.LogError( face + " : " + x + " " + y + " " + ypxB + " : " + ypyB + " : " + faceCenter + " @@@@@@@ " + _ + " : " + indexYpX + " : " + indexYpY + " : " + ypx + " : " + ypy + " : " + faceCenterYp );
                }
                yn = resultVertices[GetIndex( x, y - 1 )];
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
                    var vert = GetVertex( x, y );

                    int index = GetIndex( x, y );
#error TODO - store vertices in body-space and change them at the end only.

                    var xn = (vert.xn + originXn);// radius;
                    var xp = (vert.xp + origin);// radius;
                    var yn = (vert.yn + origin);// radius;
                    var yp = (vert.yp + origin);// radius;
                    //Vector3 tangent = (resultVertices[index] - resultVertices[index1]).normalized;
                    //Vector3 bitangent = (resultVertices[index] - resultVertices[index2]).normalized;
                    Vector3 tangent = (xp - xn).NormalizeToVector3();
                    Vector3 bitangent = (yp - yn).NormalizeToVector3();
                    Vector3 normal;
                    if( ((x == 0) ^ (y == 0)) ^ ((x == sideVertices - 1) ^ (y == sideVertices - 1)) )
                    {
                       // Debug.Log( edgeLength + " : " + (xp - xn) + " : " + (yp - yn) );
                        //normal = Vector3.Cross( bitangent, tangent );
                        normal = Vector3.Cross( tangent, bitangent );
                    }
                    else
                    {
                        normal = Vector3.Cross( bitangent, tangent );
                    }

                    resultNormals[index] = normal;
                }
            }

            // this tangents calc takes about half of the time it takes the entire thing to run.
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