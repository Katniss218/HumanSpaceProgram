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

        /// <summary>
        /// The method that generates the PQS mesh projected onto a sphere of the specified radius, with its origin at the center of the cube projected onto the same sphere.
        /// </summary>
        /// <param name="lN">How many times this mesh was subdivided (l0, l1, l2, ...).</param>
        public static Mesh GenerateCubeSphereFace( int subdivisions, float radius, Vector2 center, int lN, Vector3 origin )
        {
            // The origin of a valid, the center will never be at any of the edges of its ancestors, and will always be at the point where the inner edges of its direct children meet.

            QuadSphereFace face = QuadSphereFaceEx.FromVector( origin.normalized );

            float size = LODQuadUtils.GetSize( lN );

            int numberOfEdges = 1 << subdivisions; // Fast 2^n for integer types.
            int numberOfVertices = numberOfEdges + 1;
            float edgeLength = size / numberOfEdges; // size represents the edge length of the original square, twice the radius.
            float minX = center.x - (size / 2f); // center minus half the edge length of the cube.
            float minY = center.y - (size / 2f);

            if( numberOfVertices > 65535 )
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

                    float quadX = (i * edgeLength) + minX;
                    float quadY = (j * edgeLength) + minY;

                    Vector3Dbl posD = face.GetSpherePointDbl( quadX, quadY );
#warning TODO - subdivisions require being able to make either of the 4 edges act like it's either lN, or lN-m (if lN-m, then each other vertex will use the weighted average of the nearby vertices at lN-m).
                    // Later should be able to regenerate an edge without regenerating the entire mesh.


#warning TODO - l0 requires an additional set of vertices at Z- because UVs need to overlap on both 0.0 and 1.0 there. non-l0 require to increase the U coordinate to 1 instead of 0.
                    // EuclideanToGeodetic also returns the same value regardless, we should implement this fix here.

                    Vector3 unitSpherePos = (Vector3)posD;
                    (float latitude, float longitude, _) = CoordinateUtils.EuclideanToGeodetic( unitSpherePos );

                    float u = (latitude * Mathf.Deg2Rad + 1.5f * Mathf.PI) / (2 * Mathf.PI);
                    float v = longitude * Mathf.Deg2Rad / Mathf.PI;

                    if( (face == QuadSphereFace.Xn || face == QuadSphereFace.Zp || face == QuadSphereFace.Zn)
                      && unitSpherePos.y == 0 && unitSpherePos.x <= 0 )
                    {
                        u = 0.75f; // just setting to 0.75 doesn't work
                    }

                    uvs[index] = new Vector2( u, v );
                    vertices[index] = (Vector3)((posD * radius) - origin);

                    // Normals after displacing by heightmap will need to be calculated by hand instead of with RecalculateNormals() to avoid seams not matching up.
                    normals[index] = unitSpherePos;
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