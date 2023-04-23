using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    /// <summary>
    /// A subdivisible inflated quad.
    /// </summary>
    /// <remarks>
    /// This class does all of the heavy lifting of the system. <see cref="LODQuadSphere"/> only manages the 6 quads and tells them where the POIs are.
    /// </remarks>
    [RequireComponent( typeof( MeshFilter ) )]
    [RequireComponent( typeof( MeshRenderer ) )]
    public class LODQuad : MonoBehaviour
    {
        public LODQuad Parent { get; private set; }
        public LODQuad[] Children { get; private set; } = null;

        /// <summary>
        /// Checks if the quad is l0 (doesn't have a parent). L0 is the quad with no subdivisions (0th level of subdivision).
        /// </summary>
        public bool IsL0 { get => Parent == null; }

        public int DefaultSubdivisions { get; set; }
        public double BodyRadius { get; set; }


        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;

        // Needs to be doubles to transform this into the world space at high zoom levels without accumulated errors.
        // With floats, the position on the sphere would not be known precisely enough to place the quad with sub-meter precision for Earth.
        // - And sub-meter precision is available in the scene because of the floating origin system.
        Vector3Dbl[,] _preciseCorners = new Vector3Dbl[3, 3]; // 0,0 top-left, 0,1 top, 0,2 top-right, 1,0 left, 1,1 center, 1,2 right, 2,0 bottom-left, 2,1 bottom, 2,2 bottom-right.
        Vector3Dbl _precisePosition { get => _preciseCorners[1, 1]; set => _preciseCorners[1, 1] = value; } // center = origin.

        // Meshes are generated with high precision because the origin of the mesh is much closer to its vertices than the center of the planet would be.

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        void Start()
        {
            Mesh mesh = GeneratePartialCubeSphere( DefaultSubdivisions, (float)BodyRadius, _precisePosition );
            this.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            this.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        /// <summary>
        /// Set the <see cref="LODQuad"/> as a level 0 (root) face.
        /// </summary>
        public void SetL0( Vector3Dbl origin, int defaultSubdivisions, double bodyRadius )
        {
            if( !this.IsL0 )
            {
                throw new InvalidOperationException( "Can't set a subdivided quad as Level 0." );
            }

            _precisePosition = origin;
            DefaultSubdivisions = defaultSubdivisions;
            BodyRadius = bodyRadius;
        }

        /// <summary>
        /// Splits the specified quad into 4 separate quads.
        /// </summary>
        static void Subdivide( LODQuad q )
        {
            Vector3Dbl[] origins = new Vector3Dbl[4];
            for( int i = 0; i < 4; i++ )
            {
                int x = i % 2;
                int y = i / 2;

                // get the average lerped position of each origin and project it onto the sphere.
                origins[i] = (q._preciseCorners[x, y] + q._preciseCorners[x, y + 1] + q._preciseCorners[x + 1, y] + q._preciseCorners[x + 1, y + 1]) / 4.0;
                origins[i] = origins[i].normalized * q.BodyRadius;
            }

            // calculate the points (slerp isn't accurate with a cube-sphere, we need to get the average of 4 points and project it onto the sphere).

            // splits itself into 4 smaller quads, each with origin at avg(4 corners), protected at the sphere.
            // hide self.
        }

        /// <summary>
        /// Joins the specified quad and its 3 siblings into a single quad of lower level.
        /// </summary>
        static void Unsubdivide( LODQuad q )
        {
            if( q.IsL0 )
            {
                throw new ArgumentException( $"Can't unsubdivide a quad of level 0.", nameof( q ) );
            }

            LODQuad[] siblings = q.Parent.Children; // merge these.

            // Hide self, along with 3 siblings, and show the larger parent quad.
        }

        // Come up with some algorithm for determining when to discard the hidden (cached) levels' mesh data.

        public bool IsHidden { get => this._meshRenderer.enabled; }

        void Show()
        {
            this._meshRenderer.enabled = true;
        }

        void Hide()
        {
            this._meshRenderer.enabled = false;
        }

        /// <summary>
        /// Creates and caches the mesh for this LODQuad.
        /// </summary>
        void GenerateMeshData()
        {

        }

        /// <summary>
        /// Discards the cached mesh data for this LODQuad.
        /// </summary>
        void DiscardMeshData()
        {
            if( !IsHidden )
            {
                throw new InvalidOperationException( $"Can't discard the mesh data of an {nameof( LODQuad )} that is not hidden." );
            }

            this._meshFilter.mesh = null;
            this._meshFilter.sharedMesh = null;
        }

        /// <summary>
        /// The method that generates the PQS mesh projected onto a sphere of the specified radius, with its origin at the center of the cube projected onto the same sphere.
        /// </summary>
        static Mesh GeneratePartialCubeSphere( int subdivisions, float radius, Vector3Dbl origin )
        {
            QuadSphereFace face = QuadSphereFaceEx.FromVector( origin.normalized );
            // Origin of a valid subdivided quad can never be at the edge of any of the infinitely many theoretically possible subdivision levels.

#warning TODO - this needs a min/max specifier to determine the subdivision (i.e. don't force entire [0..1] range).
            // also useful for later, make the edge vertices align with the previous level subdivision if specified.
            // - this aligning also needs to handle existing quad islands if their neighbor subdivides - we DON'T want to regenerate the entire quad then.

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

                    (Vector3 pos, Vector3 posOffset) = CoordinateUtils.GetSpherePoint( i, j, edgeLength, radius, face );

#warning TODO - l0 requires an additional set of vertices at Z- because UVs need to overlap on both 0.0 and 1.0 there.
                    // for Zn, Yp, Yn, needs to add extra vertex for every vert with x=0

                    Vector2 uv = CoordinateUtils.CartesianToUV( pos.x, pos.z, pos.y ); // swizzle
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
