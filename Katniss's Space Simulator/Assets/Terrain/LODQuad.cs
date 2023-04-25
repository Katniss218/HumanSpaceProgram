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
        public LODQuad[] Children { get; private set; } = new LODQuad[4];

        /// <summary>
        /// Checks if the quad is l0 (doesn't have a parent). L0 is the quad with no subdivisions (0th level of subdivision).
        /// </summary>
        public bool IsL0 { get => _lN == 0; }

        /// <summary>
        /// How many binary edge subdivisions per subdiv level.
        /// </summary>
        public int EdgeSubdivisions { get; set; }

        public double CelestialBodyRadius { get; set; }

        QuadSphereFace _face;

        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;

        // Needs to be doubles to transform this into the world space at high zoom levels without accumulated errors.
        // With floats, the position on the sphere would not be known precisely enough to place the quad with sub-meter precision for Earth.
        // - And sub-meter precision is available in the scene because of the floating origin system.
        /// <summary>
        /// The precise position of the corners after projecting onto the undeformed sphere.
        /// </summary>
        Vector3Dbl[,] _preciseCorners = new Vector3Dbl[3, 3]; // 0,0 top-left, 0,1 top, 0,2 top-right, 1,0 left, 1,1 center, 1,2 right, 2,0 bottom-left, 2,1 bottom, 2,2 bottom-right.
        /// <summary>
        /// The precise position of the center (pivot) after projecting onto the undeformed sphere.
        /// </summary>
        Vector3Dbl _precisePosition { get => _preciseCorners[1, 1]; set => _preciseCorners[1, 1] = value; } // center = origin.

        // Meshes are generated with high precision because the origin of the mesh is much closer to its vertices than the center of the planet would be.

        Vector2 _center;
        int _lN;

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        private static float GetSize( int lN )
        {
            return 2.0f / (lN + 1);
        }

        /// <summary>
        /// Set the <see cref="LODQuad"/> as a level 0 (root) face.
        /// </summary>
        public void SetLN( Vector3Dbl origin, int edgeSubdivisions, double bodyRadius, Vector2 center, int lN, QuadSphereFace face )
        {
            if( !this.IsL0 )
            {
                throw new InvalidOperationException( "Can't set a subdivided quad as Level 0." );
            }

            this.EdgeSubdivisions = edgeSubdivisions;
            this.CelestialBodyRadius = bodyRadius;
            this._precisePosition = origin;
            this._center = center;
            this._lN = lN;
            this._face = face;

            // Unity keeps the local positions of objects internally.
            // - Since the origin of each LODQuad is located at the 'sea level' of each celestial body, for large celestial bodies, we might run into problems.
            // It is possible that we would want to keep the PQS parts as root objects,
            // - that way they would not be subject to precision issues caused by the large distance between their origin and their parent.
            this.transform.localPosition = (Vector3)_precisePosition;
            this.transform.localRotation = Quaternion.identity;
            this.transform.localScale = Vector3.one;

            this.GenerateMeshData();
        }

        /// <summary>
        /// Splits the specified quad into 4 separate quads.
        /// </summary>
        public static void Subdivide( LODQuad q )
        {
            float size = GetSize( q._lN );
            float halfSize = size / 2f;
            float quarterSize = size / 4f;

            Vector3Dbl[] origins = new Vector3Dbl[4];
            for( int i = 0; i < 4; i++ )
            {
                int x = i % 2;
                int y = i / 2;

                // get the average lerped position of each origin and project it onto the sphere.
                origins[i] = (q._preciseCorners[x, y] + q._preciseCorners[x, y + 1] + q._preciseCorners[x + 1, y] + q._preciseCorners[x + 1, y + 1]) / 4.0;
                origins[i] = origins[i].normalized * q.CelestialBodyRadius;

                var quad = Create( q.transform.parent, q.CelestialBodyRadius, q.EdgeSubdivisions, new Vector2( q._center.x - quarterSize + (x*halfSize), q._center.y - quarterSize + (y * halfSize)), q._lN + 1, q._face );
                q.Children[i] = quad;
                quad.Parent = q;
            }

            q.Hide();

            // calculate the points (slerp isn't accurate with a cube-sphere, we need to get the average of 4 points and project it onto the sphere).

            // splits itself into 4 smaller quads, each with origin at avg(4 corners), protected at the sphere.
            // hide self.
        }

        /// <summary>
        /// Joins the specified quad and its 3 siblings into a single quad of lower level.
        /// </summary>
        public static void Unsubdivide( LODQuad q )
        {
            if( q.IsL0 )
            {
                throw new ArgumentException( $"Can't unsubdivide a quad of level 0.", nameof( q ) );
            }

            LODQuad[] siblings = q.Parent.Children; // merge these.

            throw new NotImplementedException();
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
            Mesh mesh = GeneratePartialCubeSphere( EdgeSubdivisions, (float)CelestialBodyRadius, _center, _lN, _precisePosition ); // (0, 0) and 2 are the full quad.
            this.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            this.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
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
        /// <param name="lN">How many times this mesh was subdivided (l0, l1, l2, ...).</param>
        static Mesh GeneratePartialCubeSphere( int subdivisions, float radius, Vector2 center, int lN, Vector3Dbl origin )
        {
            // The origin of a valid, binarily subdivided quad will never be at the edge of any of the infinitely many theoretically possible subdivision levels.

            QuadSphereFace face = QuadSphereFaceEx.FromVector( origin.normalized );

            float size = GetSize( lN );

            int numberOfEdges = 1 << subdivisions; // fast 2^n
            int numberOfVertices = numberOfEdges + 1;
            float edgeLength = size / numberOfEdges; // size represents the edge length of the original square, twice the radius.
            float minX = center.x - (size / 2f); // center minus half the edge length of the cube.
            float minY = center.y - (size / 2f);

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

                    (Vector3 pos, Vector3 posOffset) = MeshUtils.GetSpherePoint( i, j, edgeLength, minX, minY, face );

#warning TODO - l0 requires an additional set of vertices at Z- because UVs need to overlap on both 0.0 and 1.0 there.
                    // for Zn, Yp, Yn, needs to add extra vertex for every vert with x=0

                    Vector3 lla = CoordinateUtils.EuclideanToGeodetic( pos.x, pos.y, pos.z );
                    uvs[index] = new Vector2( (lla.x * Mathf.Deg2Rad + 1.5f * Mathf.PI) / (2 * Mathf.PI), lla.y * Mathf.Deg2Rad / Mathf.PI );

                    vertices[index] = (pos * radius) - (posOffset * radius);
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

        public static LODQuad Create( Transform parent, double celestialBodyRadius, int defaultSubdivisions, Vector2 center, int lN, QuadSphereFace face )
        {
            GameObject go = new GameObject( $"LODQuad L{lN}, {face}, {center}" );
            go.transform.SetParent( parent );

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = FindObjectOfType<zzzTestGameManager>().CBMaterial;

            go.AddComponent<MeshCollider>();

#warning TODO - offset from lN.
            Vector3 dir = face.ToVector3();
            Vector3Dbl offset = ((Vector3Dbl)dir) * celestialBodyRadius;

            LODQuad q = go.AddComponent<LODQuad>();
            q.SetLN( offset, defaultSubdivisions, celestialBodyRadius, center, lN, face );

            return q;
        }
    }
}