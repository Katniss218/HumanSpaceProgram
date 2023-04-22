using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    public class MeshGeneratorTest : MonoBehaviour
    {
        // With floats, we get 1 meter of precision for vertices 10^6 meters away from the origin.
        // This is not adequate, thus the more subdivided the PQS islands are (closer to camera/vessels), the closer their vertices have to be to the origin.

        //      CONCLUSIONS:
        /*
            - Earth-sized quadrilateral mesh collider appears to work. It got a bit funky when I rotated it though, the rigidbody seemed to think it was still colliding.


        */

        [SerializeField]
        Vector3[] vertices = new Vector3[20];
        int[] triangles;

        float radius = 6371011.123456f;

        [SerializeField]
        MeshCollider[] col;

        [SerializeField]
        MeshFilter[] mf;

        // Start is called before the first frame update
        void Start()
        {
            col = this.GetComponentsInChildren<MeshCollider>();
            mf = this.GetComponentsInChildren<MeshFilter>();

            for( int i = 0; i < 6; i++ )
            {
                Mesh mesh = new CubeSphereTerrain().GeneratePartialCubeSphere( 5, 10, (CubeSphereTerrain.Face)i );
                col[i].sharedMesh = mesh;
                mf[i].sharedMesh = mesh;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        Vector3 GetFaceNormal( Vector3 v1, Vector3 v2, Vector3 v3 )
        {
            return Vector3.Cross( v1 - v2, v3 - v2 );
        }

        Mesh MakeQuad()
        {
            const float radius = 6371011.123456f; // earth.

            vertices = new Vector3[4];
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
            triangles = new int[6];
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
    }
}