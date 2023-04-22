using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    public class CubeSphereTerrain
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

        public Mesh GeneratePartialCubeSphere( int subdivisions, Face face )
        {
            const float radius = 10.0f;
            const float halfRadius = radius / 2;

            int numberOfEdges = 1 << subdivisions; // fast 2^n
            int numberOfVertices = numberOfEdges + 1;
            float edgeLength = radius / numberOfEdges;

            Vector3[] vertices = new Vector3[numberOfVertices * numberOfVertices];
            Vector3[] normals = new Vector3[numberOfVertices * numberOfVertices];
            Vector2[] uvs = new Vector2[numberOfVertices * numberOfVertices];

            for( int i = 0; i < numberOfVertices; i++ )
            {
                for( int j = 0; j < numberOfVertices; j++ )
                {
                    int index = (i * numberOfEdges) + i + j;

                    Vector3 pos;
                    switch( face )
                    {
                        case Face.Xp:
                            pos = new Vector3( halfRadius, (j * edgeLength) - halfRadius, (i * edgeLength) - halfRadius ); break;
                        case Face.Xn:
                            pos = new Vector3( -halfRadius, (i * edgeLength) - halfRadius, (j * edgeLength) - halfRadius ); break;
                        case Face.Yp:
                            pos = new Vector3( (i * edgeLength) - halfRadius, halfRadius, (j * edgeLength) - halfRadius ); break;
                        case Face.Yn:
                            pos = new Vector3( (j * edgeLength) - halfRadius, -halfRadius, (i * edgeLength) - halfRadius ); break;
                        case Face.Zp:
                            pos = new Vector3( (j * edgeLength) - halfRadius, (i * edgeLength) - halfRadius, halfRadius ); break;
                        case Face.Zn:
                            pos = new Vector3( (i * edgeLength) - halfRadius, (j * edgeLength) - halfRadius, -halfRadius ); break;
                        default:
                            throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
                    }
                    pos.Normalize(); // unit sphere.

#warning TODO - requires additional set of vertices at Z- because UVs need to overlap on both 0.0 and 1.0 there.
                    // for Zn, Yp, Yn, needs to add extra vertex for every vert with x=0


                    float latitude = Mathf.Acos( pos.y ) / Mathf.PI;
                    float longitude = (Mathf.Atan2( pos.x, pos.z ) + Mathf.PI) / (2 * Mathf.PI);
                    uvs[index] = new Vector2( longitude, latitude );

                    vertices[index] = pos * radius;
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