using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
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
        // Generated meshes have relatively higher precision because the vertices are closer to the origin of the mesh, than to the origin of the celestial body.

        public const int HARD_LIMIT_LN = 10;

        public LODQuadTree.Node Node { get; set; }

        /// <summary>
        /// How many binary edge subdivisions per subdiv level.
        /// </summary>
        public int EdgeSubdivisions { get; private set; }

        LODQuadSphere _quadSphere;

        public CelestialBody CelestialBody { get; private set; }

        public float SubdivisionDistance { get; private set; }

        QuadSphereFace _face;

        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;

        public Vector3Dbl? airfPOI { get; set; }

        // 2D center of the quad
        Vector2 _center;
        // subdiv level
        [field: SerializeField]
        public int LN { get; private set; }

        /// <summary>
        /// Checks if the quad is l0 (doesn't have a parent). L0 is the quad with no subdivisions (0th level of subdivision).
        /// </summary>
        public bool IsL0 { get => LN == 0; }

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if( airfPOI == null )
            {
                if( !this.IsL0 )
                {
                    //  Unsubdivide( this );
                }
                return;
            }

            Vector3Dbl airfQuad = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position );
            double dist = (airfPOI.Value - airfQuad).magnitude;
            if( (float)dist < SubdivisionDistance )
            {
                if( this.LN < HARD_LIMIT_LN )
                {
                    Subdivide( this );
                }
                return;
            }

            if( this.LN > 0 )
            {
                foreach( var siblingNode in this.Node.Parent.Children )
                {
#warning TODO - doesn't catch correctly.
                    if( siblingNode.Children != null )
                        return; // one of the siblings is subdivided
                }

                // if distance to would-be-parent is more than its subdiv radius
                Vector2 center = this.GetSiblingCenter();
                Vector3 originBodySpace = MeshUtils.GetSpherePoint( center.x, center.y, _face ) * (float)CelestialBody.Radius;

                Vector3Dbl airfPosQuadParent = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.CelestialBody.transform.TransformPoint( originBodySpace ) );
                double dist2 = (airfPOI.Value - airfPosQuadParent).magnitude;
                if( dist2 > this.SubdivisionDistance * 2 )
                {
                    Unsubdivide( this );
                }
                return;
            }
        }

        private static float GetSize( int lN )
        {
            float s = 2.0f;
            while( lN > 0 )
            {
                s /= 2f;
                lN--;
            }
            return s;
        }

        private Vector2 GetSiblingCenter()
        {
            double centerX = 0;
            double centerY = 0;

            foreach( var qSibling in this.Node.Parent.Children )
            {
                centerX += qSibling.Value._center.x;
                centerY += qSibling.Value._center.y;
            }

            centerX /= 4.0;
            centerY /= 4.0;
            return new Vector2( (float)centerX, (float)centerY );
        }

        /// <summary>
        /// Set the <see cref="LODQuad"/> as a level 0 (root) face.
        /// </summary>
        public void SetLN( Vector3 origin, int edgeSubdivisions, Vector2 center, int lN, QuadSphereFace face )
        {
            this.EdgeSubdivisions = edgeSubdivisions;
            this._center = center;
            this.LN = lN;
            this._face = face;

            // Unity keeps the local positions of objects internally.
            // - Since the origin of each LODQuad is located at the 'sea level' of each celestial body, for large celestial bodies, we might run into problems.
            // It is possible that we would want to keep the PQS parts as root objects,
            // - that way they would not be subject to precision issues caused by the large distance between their origin and their parent.
            this.transform.localPosition = origin;
            this.transform.localRotation = Quaternion.identity;
            this.transform.localScale = Vector3.one;

            this.GenerateMeshData();
        }

        /// <summary>
        /// Splits the specified quad into 4 separate quads.
        /// </summary>
        public static void Subdivide( LODQuad q )
        {
            if( q.LN >= HARD_LIMIT_LN )
            {
                return;
            }
            if( q.Node.Children != null )
            {
                Debug.LogWarning( "Tried subdividing a subdivided node" );
                return;
            }

            float size = GetSize( q.LN );
            float halfSize = size / 2f;
            float quarterSize = size / 4f;

            q.Node.Children = new LODQuadTree.Node[2, 2];

            for( int i = 0; i < 4; i++ )
            {
                int x = i % 2;
                int y = i / 2;

                Vector2 center = new Vector2( q._center.x - quarterSize + (x * halfSize), q._center.y - quarterSize + (y * halfSize) );

                Vector3 origin = MeshUtils.GetSpherePoint( center.x, center.y, q._face ) * (float)q.CelestialBody.Radius;

                var quad = Create( q.transform.parent, origin, q._quadSphere, q.CelestialBody, q.EdgeSubdivisions, center, q.LN + 1, q.SubdivisionDistance / 2f, q._face );

                q.Node.Children[x, y] = new LODQuadTree.Node() { Value = quad, Parent = q.Node };
                quad.Node = q.Node.Children[x, y];
            }

            q.airfPOI = null;
            q.Node.Value = null;

            Destroy( q.gameObject );
            // q.Hide();
        }

        /// <summary>
        /// Joins the specified quad and its 3 siblings into a single quad of lower level.
        /// </summary>
        static void Unsubdivide( LODQuad q )
        {
            if( q.LN <= 0 )
            {
                Debug.LogWarning( "Tried subdividing an l0 node" );
                return;
            }

            foreach( var siblingNode in q.Node.Parent.Children )
            {
                if( siblingNode.Children != null )
                    return; // one of the siblings is subdivided
            }

            Vector2 center = q.GetSiblingCenter(); // this is good.
            Vector3 origin = MeshUtils.GetSpherePoint( center.x, center.y, q._face ) * (float)q.CelestialBody.Radius; // good too
            var quad = Create( q.transform.parent, origin, q._quadSphere, q.CelestialBody, q.EdgeSubdivisions, center, q.LN - 1, q.SubdivisionDistance * 2f, q._face );
            quad.airfPOI = q.airfPOI; // VERY IMPORTANT (I don't like that it is). without it, the update will snatch it up and unsubdivide again and again and again.

            quad.Node = q.Node.Parent;
            // when unsubdividing, the children don't get reset correctly?
            //quad.Node.Parent.Children[???] = quad;

            foreach( var qSibling in q.Node.Parent.Children )
            {
                qSibling.Value.airfPOI = null;
                // qSibling.Hide();
                Destroy( qSibling.Value.gameObject );
            }

            q.Node.Parent.Children = null;

            // q.Hide();
            // quadtree structure to store the hierarchy? that should make it easy to calc.

            //q.Parent.Show();

            // Hide self, along with 3 siblings, and show the larger parent quad.
        }

        // Come up with some algorithm for determining when to discard the hidden (cached) levels' mesh data.

        public bool IsHidden { get => this.gameObject.activeSelf; }

        void Show()
        {
            this.gameObject.SetActive( true );
        }

        void Hide()
        {
            this.gameObject.SetActive( false );
        }

        /// <summary>
        /// Creates and caches the mesh for this LODQuad.
        /// </summary>
        void GenerateMeshData()
        {
            Mesh mesh = GeneratePartialCubeSphere( EdgeSubdivisions, (float)CelestialBody.Radius, _center, LN, this.transform.localPosition ); // (0, 0) and 2 are the full quad.
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
        static Mesh GeneratePartialCubeSphere( int subdivisions, float radius, Vector2 center, int lN, Vector3 origin )
        {
            // The origin of a valid, binarily subdivided quad will never be at the edge of any of the infinitely many theoretically possible subdivision levels.

            // If converted to doubles internally, it should be more precise and match positions relative to body center for all subdiv levels (origin should remain as float vector though).

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
                    float quadX = (i * edgeLength) + minX;
                    float quadY = (j * edgeLength) + minY;

                    Vector3 pos = MeshUtils.GetSpherePoint( quadX, quadY, face );

#warning TODO - l0 requires an additional set of vertices at Z- because UVs need to overlap on both 0.0 and 1.0 there. 
                    // EuclideanToGeodetic also returns the same value regardless, we should check here.
                    // for Zn, Yp, Yn, needs to add extra vertex for every vert with x=0

                    Vector3 lla = CoordinateUtils.EuclideanToGeodetic( pos.x, pos.y, pos.z );

                    int index = (i * numberOfEdges) + i + j;

                    uvs[index] = new Vector2( (lla.x * Mathf.Deg2Rad + 1.5f * Mathf.PI) / (2 * Mathf.PI), lla.y * Mathf.Deg2Rad / Mathf.PI );

                    vertices[index] = (pos * radius) - origin;
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

        public static LODQuad Create( Transform parent, Vector3 localPosition, LODQuadSphere quadSphere, CelestialBody celestialBody, int defaultSubdivisions, Vector2 center, int lN, float subdivisionDistance, QuadSphereFace face )
        {
            GameObject go = new GameObject( $"LODQuad L{lN}, {face}, ({center.x:#0.################}, {center.y:#0.################})" );
            go.transform.SetParent( parent );

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = FindObjectOfType<zzzTestGameManager>().CBMaterial;

            go.AddComponent<MeshCollider>();

            LODQuad q = go.AddComponent<LODQuad>();
            q.CelestialBody = celestialBody;
            q._quadSphere = quadSphere;
            q.SubdivisionDistance = subdivisionDistance;
            q.SetLN( localPosition, defaultSubdivisions, center, lN, face );

            return q;
        }
    }
}