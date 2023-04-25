using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    /// <summary>
    /// A class grouping helper methods relating to geometry and meshes.
    /// </summary>
    public static class MeshUtils
    {
        /// <summary>
        /// Calculates the surface normal of a triangle.
        /// </summary>
        [Obsolete( "Requires testing to determine if the order of the vectors in the cross product is correct, or if it should be flipped, to match the behaviour of Unity and clockwise point order." )]
        public static Vector3 GetFaceNormal( Vector3 v1, Vector3 v2, Vector3 v3 )
        {
            return Vector3.Cross( v1 - v2, v3 - v2 ).normalized;
        }

        /// <summary>
        /// Makes a quad with 4 vertices and 2 triangles.
        /// </summary>
        public static Mesh MakeQuad( float radius )
        {
            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];

            vertices[0] = new Vector3( 0, 0, 0 );
            vertices[1] = new Vector3( 1, 0, 0 );
            vertices[2] = new Vector3( 1, 0, 1 );
            vertices[3] = new Vector3( 0, 0, 1 );

            for( int i = 0; i < vertices.Length; i++ )
            {
                vertices[i] *= radius;
            }

            // Counter-Clockwise when looking towards the triangle. Faces away.

            // Clockwise when looking towards the triangle. Faces you.
            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;
            triangles[3] = 1;
            triangles[4] = 3;
            triangles[5] = 2;

            Mesh mesh = new Mesh();
            mesh.SetVertices( vertices );
            mesh.SetTriangles( triangles, 0 );
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the normalized point on the surface of a cube, and the vector transforming the center-origin coordinates into face-origin.</returns>
        public static Vector3 GetSpherePoint( float quadX, float quadY, QuadSphereFace face )
        {
            // quad x, y go in range [-1..1]
            Vector3 pos;
            switch( face )
            {
                case QuadSphereFace.Xp:
                    pos = new Vector3( 1.0f, quadY, quadX );
                    break;
                case QuadSphereFace.Xn:
                    pos = new Vector3( -1.0f, quadX, quadY );
                    break;
                case QuadSphereFace.Yp:
                    pos = new Vector3( quadX, 1.0f, quadY );
                    break;
                case QuadSphereFace.Yn:
                    pos = new Vector3( quadY, -1.0f, quadX );
                    break;
                case QuadSphereFace.Zp:
                    pos = new Vector3( quadY, quadX, 1.0f );
                    break;
                case QuadSphereFace.Zn:
                    pos = new Vector3( quadX, quadY, -1.0f );
                    break;
                default:
                    throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
            }

            pos.Normalize(); // unit sphere.
            return pos;
        }
    }
}