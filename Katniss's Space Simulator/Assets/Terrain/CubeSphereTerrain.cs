using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    public static class CubeSphereTerrain
    {
        // Simpler PQS-like generation for now.
        // we can use the more elaborate face-normal version later if needed.


        // We need a way to index the different pieces to tell the generator which piece and how large (subdiv no.) to generate.

        // generator generates pieces with equal vert count, higher subdiv are smaller.

        public enum Face
        {
            Xp,
            Xn,
            Yp,
            Yn,
            Zp,
            Zn
        }

        static Vector3 GetFaceNormal( Vector3 v1, Vector3 v2, Vector3 v3 )
        {
            return Vector3.Cross( v1 - v2, v3 - v2 );
        }

        static Mesh MakeQuad()
        {
            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];

            const float radius = 6371011.123456f; // earth.

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

        public static Vector3 UVToCartesian( float u, float v, float radius )
        {
            float theta = u * (2 * Mathf.PI) - Mathf.PI; // Multiplying by 2 because the input is in range [0..1]
            float phi = v * Mathf.PI;

            float x = radius * Mathf.Sin( phi ) * Mathf.Cos( theta );
            float y = radius * Mathf.Sin( phi ) * Mathf.Sin( theta );
            float z = radius * Mathf.Cos( phi );
            return new Vector3( x, y, z );
        }

        /// <summary>
        /// Z+ is up (V+), seam is in the direction of X-.
        /// U increases from 0 on the Y- side of the seam, decreases from 1 on the Y+ side.
        /// </summary>
        public static Vector2 CartesianToUV( float x, float y, float z )
        {
            // If we know for sure that the coordinates are for a unit sphere, we can remove the radius calculation entirely.
            // Also remove the division by radius, since division by 1 (unit-sphere) doesn't change the divided number.
            float radius = Mathf.Sqrt( x * x + y * y + z * z );
            float theta = Mathf.Atan2( y, x );
            float phi = Mathf.Acos( z / radius );

            // The thing returned here seems to also be the lat/lon but normalized to the range [0..1]
            // The outputs are normalized by the respective denominators.
            float u = (theta + Mathf.PI) / (2 * Mathf.PI); // dividing by 2 * pi ensures that the output is in [0..1]
            float v = phi / Mathf.PI;
            return new Vector2( u, v );
        }

        private static (Vector3 pos, Vector3 posOffset) GetSpherePoint( int i, int j, float edgeLength, float radius, Face face )
        {
            Vector3 pos;
            Vector3 posOffset;
            switch( face )
            {
                case Face.Xp:
                    pos = new Vector3( radius, (j * edgeLength) - radius, (i * edgeLength) - radius );
                    posOffset = new Vector3( radius, 0, 0 );
                    break;
                case Face.Xn:
                    pos = new Vector3( -radius, (i * edgeLength) - radius, (j * edgeLength) - radius );
                    posOffset = new Vector3( -radius, 0, 0 );
                    break;
                case Face.Yp:
                    pos = new Vector3( (i * edgeLength) - radius, radius, (j * edgeLength) - radius );
                    posOffset = new Vector3( 0, radius, 0 );
                    break;
                case Face.Yn:
                    pos = new Vector3( (j * edgeLength) - radius, -radius, (i * edgeLength) - radius );
                    posOffset = new Vector3( 0, -radius, 0 );
                    break;
                case Face.Zp:
                    pos = new Vector3( (j * edgeLength) - radius, (i * edgeLength) - radius, radius );
                    posOffset = new Vector3( 0, 0, radius );
                    break;
                case Face.Zn:
                    pos = new Vector3( (i * edgeLength) - radius, (j * edgeLength) - radius, -radius );
                    posOffset = new Vector3( 0, 0, -radius );
                    break;
                default:
                    throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
            }
            pos.Normalize(); // unit sphere.
            return (pos, posOffset);
        }

        public static Mesh GeneratePartialCubeSphere( int subdivisions, float radius, Face face )
        {
            float diameter = radius * 2;

            int numberOfEdges = 1 << subdivisions; // fast 2^n
            int numberOfVertices = numberOfEdges + 1;
            float edgeLength = diameter / numberOfEdges;

            if( subdivisions > 7 )
            {
                // technically wrong, since Mesh.indexFormat can be switched to 32 bit, but i'll leave this for now. Meshes don't have to be over that value anyway because laggy and big and far away.
                throw new ArgumentOutOfRangeException( $"Unity's Mesh can contain at most 65535 vertices (16-bit buffer). Tried to create a Mesh with {numberOfVertices}." );
            }

            Vector3[] vertices = new Vector3[numberOfVertices * numberOfVertices];
            Vector3[] normals = new Vector3[numberOfVertices * numberOfVertices];
            Vector2[] uvs = new Vector2[numberOfVertices * numberOfVertices];

            for( int i = 0; i < numberOfVertices; i++ )
            {
                for( int j = 0; j < numberOfVertices; j++ )
                {
                    int index = (i * numberOfEdges) + i + j;

                    (Vector3 pos, Vector3 posOffset) = GetSpherePoint( i, j, edgeLength, radius, face );

#warning TODO - requires additional set of vertices at Z- because UVs need to overlap on both 0.0 and 1.0 there.
                    // for Zn, Yp, Yn, needs to add extra vertex for every vert with x=0

                    Vector2 uv = CartesianToUV( pos.x, pos.z, pos.y ); // swizzle
                    uvs[index] = new Vector2( 1 - uv.x, uv.y );

                    vertices[index] = pos * radius - posOffset;
                    normals[index] = pos; // Normals need to be calculated by hand to avoid seams not matching up.
                }
            }

            List<int> triangles = new List<int>();
            for( int i = 0; i < numberOfEdges; i++ )
            {
                for( int j = 0; j < numberOfEdges; j++ )
                {
                    int index = (i * numberOfEdges) + i + j;

                    //   C - D
                    //   | / |
                    //   A - B

                    // Adding numberOfVertices makes it skip to the next row (number of vertices is 1 higher than edges).
                    triangles.Add( index ); // A
                    triangles.Add( index + numberOfVertices + 1 ); // D
                    triangles.Add( index + numberOfVertices ); // C

                    triangles.Add( index ); // A
                    triangles.Add( index + 1 ); // B
                    triangles.Add( index + numberOfVertices + 1 ); // D
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals;
            mesh.uv = uvs; // UVs are harder, requires spherical coordinates and transforming from the planet origin to the mesh island origin.
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}