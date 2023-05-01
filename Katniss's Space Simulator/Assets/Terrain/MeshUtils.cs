using KatnisssSpaceSimulator.Core;
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

        public static void FixTangents( this Mesh mesh )
        {
            // For SOME REASON, tangents are fucked (especially on subdivision levels > 6)
            // - I have no idea why builtin solver can't handle them...
            // At some point, some tangents end up with positive `w`. If we flip those, everything seems to look okay again.
            // - I don't know why. Maybe not curvy enough? idfk...

            Vector4[] tang = mesh.tangents; // this is fast.
            for( int i = 0; i < tang.Length; i++ )
            {
                if( tang[i].w == 1.0f )
                {
                    tang[i] = -tang[i]; // flipping only w doesn't fix how the mesh looks.
                }
            }
            mesh.SetTangents( tang ); // this is fast.
        }

        public static void FlipTangents( this Mesh mesh )
        {
            Vector4[] tang = mesh.tangents;
            for( int i = 0; i < tang.Length; i++ )
            {
                tang[i] = -tang[i];
            }
            mesh.SetTangents( tang );
        }

        public static void CalculateMeshTangents( this Mesh mesh )
        {
            //speed up math by copying the mesh arrays
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = mesh.uv;
            Vector3[] normals = mesh.normals;

            //variable definitions
            int triangleCount = triangles.Length;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Vector4[] tangents = new Vector4[vertexCount];

            for( int i = 0; i < triangleCount; i += 3 )
            {
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 uv1 = uv[i1];
                Vector2 uv2 = uv[i2];
                Vector2 uv3 = uv[i3];

                float v1tov2X = v2.x - v1.x;
                float v1tov3X = v3.x - v1.x;
                float v1tov2Y = v2.y - v1.y;
                float v1tov3Y = v3.y - v1.y;
                float v1tov2Z = v2.z - v1.z;
                float v1tov3Z = v3.z - v1.z;

                float uv1ToUv2X = uv2.x - uv1.x;
                float uv1ToUv3X = uv3.x - uv1.x;
                float uv1ToUv2Y = uv2.y - uv1.y;
                float uv1ToUv3Y = uv3.y - uv1.y;

                // This seems to mostly work, but this line causes issues with very high subdiv levels.
                float div = uv1ToUv2X * uv1ToUv3Y - uv1ToUv3X * uv1ToUv2Y;
                float r = div == 0.0f ? 0f : 1.0f / div;

                Vector3 sdir = new Vector3( (uv1ToUv3Y * v1tov2X - uv1ToUv2Y * v1tov3X) * r, (uv1ToUv3Y * v1tov2Y - uv1ToUv2Y * v1tov3Y) * r, (uv1ToUv3Y * v1tov2Z - uv1ToUv2Y * v1tov3Z) * r );
                Vector3 tdir = new Vector3( (uv1ToUv2X * v1tov3X - uv1ToUv3X * v1tov2X) * r, (uv1ToUv2X * v1tov3Y - uv1ToUv3X * v1tov2Y) * r, (uv1ToUv2X * v1tov3Z - uv1ToUv3X * v1tov2Z) * r );

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for( int i = 0; i < vertexCount; ++i )
            {
                Vector3 normal = normals[i];
                Vector3 tangent = tan1[i];

                Vector3.OrthoNormalize( ref normal, ref tangent );
                tangents[i].x = tangent.x;
                tangents[i].y = tangent.y;
                tangents[i].z = tangent.z;

                // w is used to store "handedness" of the binormal.
                // 2023/04/30 I've found that the handedness is never positive when the tangents generated by Unity look correct.
                tangents[i].w = (Vector3.Dot( Vector3.Cross( normal, tangent ), tan2[i] ) < 0.0f) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }
    }
}