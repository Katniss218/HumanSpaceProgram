using HSP.CelestialBodies.Surfaces;
using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vanilla.CelestialBodies
{
    /// <summary>
    /// A job that constructs the base mesh for the terrain.
    /// </summary>
    public struct MakeQuadMesh_Job : ILODQuadJob
    {
        double radius;
        Vector3Dbl origin;
        float size;
        float edgeLength;
        float minX;
        float minY;
        Direction3D face;

        int sideEdges;
        int sideVertices;

        NativeArray<Vector3> resultVertices;
        NativeArray<Vector3> resultNormals;
        NativeArray<Vector2> resultUvs;
        NativeArray<int> resultTriangles;

        public LODQuadMode QuadMode => LODQuadMode.VisualAndCollider;

        public void Initialize( LODQuadRebuildData r )
        {
#warning TODO - keep the verts in body space (as vector3dbl), turn into mesh-space when finalizing.
            radius = (float)r.CelestialBody.Radius;
            origin = r.Node.SphereCenter * radius;
            size = r.Node.Size;
            face = r.Node.Face;

            sideEdges = r.SideEdges;
            sideVertices = r.SideVertices;
            edgeLength = size / sideEdges; // size represents the edge length of the original square, twice the radius.
            minX = r.Node.FaceCenter.x - (size / 2f); // center minus half the edge length of the cube.
            minY = r.Node.FaceCenter.y - (size / 2f);

            resultVertices = r.ResultVertices;
            resultNormals = r.ResultNormals;
            resultUvs = r.ResultUVs;
            resultTriangles = r.ResultTriangles;
        }

        public void Finish( LODQuadRebuildData r )
        {
            r.Mesh.SetVertices( resultVertices );
            r.Mesh.SetNormals( resultNormals );
            r.Mesh.SetUVs( 0, resultUvs );
            r.Mesh.SetTriangles( resultTriangles.ToArray(), 0 );
        }

        public void Dispose()
        {
        }

        public ILODQuadJob Clone()
        {
            return new MakeQuadMesh_Job();
        }

        public void Execute() // Called by Unity from a job thread.
        {
            GenerateCubeSphereFace();
        }

        int GetIndex( int x, int y )
        {
            return (x * sideEdges) + x + y;
        }

        /// <summary>
        /// The method that generates the PQS mesh projected onto a sphere of the specified radius, with its origin at the center of the cube projected onto the same sphere.
        /// </summary>
        /// <param name="lN">How many times this mesh was subdivided (l0, l1, l2, ...).</param>
        public void GenerateCubeSphereFace()
        {
            // The origin of a valid, the center will never be at any of the edges of its ancestors, and will always be at the point where the inner edges of its direct children meet.

            if( sideVertices > 65535 )
            {
                // technically wrong, since Mesh.indexFormat can be switched to 32 bit, but i'll leave this for now. Meshes don't have to be over that value anyway because laggy and big and far away.
                throw new ArgumentOutOfRangeException( $"Unity's Mesh can contain at most 65535 vertices (16-bit buffer). Tried to create a Mesh with {sideVertices}." );
            }

            int triIndex = 0;
            for( int x = 0; x < sideEdges; x++ )
            {
                for( int y = 0; y < sideEdges; y++ )
                {
                    int index = GetIndex( x, y );

                    //   C - D
                    //   | / |
                    //   A - B

                    // Adding numberOfVertices makes it skip to the next row (number of vertices is 1 higher than edges).
                    resultTriangles[triIndex] = index; // A
                    resultTriangles[triIndex + 1] = index + sideVertices + 1; // D
                    resultTriangles[triIndex + 2] = index + sideVertices; // C

                    resultTriangles[triIndex + 3] = index; // A
                    resultTriangles[triIndex + 4] = index + 1; // B
                    resultTriangles[triIndex + 5] = index + sideVertices + 1; // D

#warning TODO - we'll need additional 'fake' triangles to compute normals, and they might have problems at the seams of the 6 primary faces because of Direction3D.GetSpherePointDbl.
                    // so this needs to know if any of the 4 edges match the edges of the L0 quad.

                    // would be benefitial to get neighboring meshes to talk to each other.

                    // maybe abandon the direction3d stuff and use a different way to encode the position of the quad, equirectangular? spherical coords?


#warning TODO - we could do it by 'creating the new meshes' kind of 'on the side', and swapping the existing quads once the new meshes are 'ready'

                    // this could allow swapping (updating) arbitrary areas of terrain 'at once', instead of waiting for quads to recursively subdivide themselves
                    // - previous algorithm needs the previous quad to exist to subdivide it into 4

                    // we could keep trach of which faces (leaves) we want, and which faces we have
                    // compute the difference and figure out which faces need to change
                    // when swapping, update which faces we have.

                    // when initializing, let the mods retrieve all meshes? (they will only use the ones they need, but all may be available)

#warning TODO - check if the jobs can access the same container one after the other. If not, .NET threading will have to be used (task/async)

                    triIndex += 6;
                }
            }

            for( int x = 0; x < sideVertices; x++ )
            {
                for( int y = 0; y < sideVertices; y++ )
                {
                    int index = GetIndex( x, y );

                    float quadX = (x * edgeLength) + minX; // This might need to be turned into a double perhaps (for large bodies with lots of subdivs).
                    float quadY = (y * edgeLength) + minY;

                    Vector3Dbl posD = face.GetSpherePointDbl( quadX, quadY );

                    Vector3 normal = (Vector3)posD;

                    resultVertices[index] = (Vector3)((posD * radius) - origin);

                    const float margin = 0.0f; // margin can be 0 when the texture wrap mode is set to mirror.
                    resultUvs[index] = new Vector2( quadX * (0.5f - margin) + 0.5f, quadY * (0.5f - margin) + 0.5f );
#warning INFO - 'lerping' it like that introduces stretch, the cubemap should be counter-stretched to cancel it.

                    // Normals after displacing by heightmap will need to be calculated by hand instead of with RecalculateNormals() to avoid seams not matching up.
                    // normals can be calculated by adding the normals of each face to its vertices, then normalizing.
                    // - this will compute smooth VERTEX normals!!
                    resultNormals[index] = normal;
                }
            }
        }
    }
}