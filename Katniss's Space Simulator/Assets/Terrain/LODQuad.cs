using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
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
    [RequireComponent( typeof( MeshCollider ) )]
    [RequireComponent( typeof( MeshRenderer ) )]
    public class LODQuad : MonoBehaviour
    {
        // Generated meshes have relatively higher precision because the vertices are closer to the origin of the mesh, than to the origin of the celestial body.

        /// <summary>
        /// The level of subdivision (lN) at which the quad will stop subdividing.
        /// </summary>
        public int HardLimitSubdivLevel { get; set; } = 20;

        public LODQuadTree.Node Node { get; set; }

        /// <summary>
        /// How many edge subdivision steps to apply for each quad, on top of and regardless of <see cref="SubdivisionLevel"/>.
        /// </summary>
        public int EdgeSubdivisions { get; private set; }

        LODQuadSphere _quadSphere;

        public CelestialBody CelestialBody { get; private set; }

        public float SubdivisionDistance { get; private set; }

        QuadSphereFace _face;

        MeshFilter _meshFilter;
        MeshCollider _meshCollider;
        MeshRenderer _meshRenderer;

        public Vector3Dbl? airfPOI { get; set; }

        [field: SerializeField]
        public int SubdivisionLevel { get; private set; }

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshCollider = this.GetComponent<MeshCollider>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if( airfPOI == null )
            {
                if( this.SubdivisionLevel > 0 )
                {
                    // Should immediately unsubdivide all the way up to l0 if no POI is present.
                    //  Unsubdivide( this );
                }
                return;
            }

            Vector3Dbl airfQuad = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position );
            double dist = (airfPOI.Value - airfQuad).magnitude;
            if( (float)dist < SubdivisionDistance )
            {
                if( this.SubdivisionLevel < HardLimitSubdivLevel )
                {
                    Subdivide( this );
                }
                return;
            }

            if( this.SubdivisionLevel > 0 )
            {
                foreach( var siblingNode in this.Node.Siblings )
                {
                    if( siblingNode.Children != null )
                    {
                        return; // one of the siblings is subdivided
                    }
                }

                // if distance to would-be-parent is more than its subdiv radius
                Vector2 center = this.Node.Parent.Center;
                Vector3 originBodySpace = _face.GetSpherePoint( center.x, center.y ) * (float)CelestialBody.Radius;

                Vector3Dbl airfPosQuadParent = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.CelestialBody.transform.TransformPoint( originBodySpace ) );
                double dist2 = (airfPOI.Value - airfPosQuadParent).magnitude;
                if( dist2 > this.SubdivisionDistance * 2 )
                {
                    Unsubdivide( this );
                }
                return;
            }
        }

        /// <summary>
        /// Splits the specified quad into 4 separate quads.
        /// </summary>
        static void Subdivide( LODQuad q )
        {
            if( q.Node.Children != null )
            {
                throw new InvalidOperationException( "Tried subdividing a subdivided node" );
            }

            if( q.SubdivisionLevel >= q.HardLimitSubdivLevel )
            {
                return;
            }

            q.Node.Children = new LODQuadTree.Node[2, 2];

            for( int i = 0; i < 4; i++ )
            {
                (int xIndex, int yIndex) = LODQuadUtils.GetChildIndex( i );

                Vector2 center = LODQuadUtils.GetChildCenter( q.Node, xIndex, yIndex );

                Vector3 origin = q._face.GetSpherePoint( center.x, center.y ) * (float)q.CelestialBody.Radius;

                int lN = q.SubdivisionLevel + 1;
                q.Node.Children[xIndex, yIndex] = new LODQuadTree.Node()
                {
                    Center = center,
                    Size = LODQuadUtils.GetSize( lN ),
                    Parent = q.Node
                };

                LODQuad quad = Create( q.transform.parent, origin, q._quadSphere, q.CelestialBody, q.EdgeSubdivisions, center, lN, q.Node.Children[xIndex, yIndex], q.SubdivisionDistance / 2f, q._face );
            }

            q.airfPOI = null;
            q.Node.Value = null;

            Destroy( q.gameObject );
        }

        /// <summary>
        /// Joins the specified quad and its 3 siblings into a single quad of lower level.
        /// </summary>
        static void Unsubdivide( LODQuad q )
        {
            if( q.SubdivisionLevel <= 0 )
            {
                Debug.LogWarning( "Tried subdividing an l0 node" );
                return;
            }

            foreach( var siblingNode in q.Node.Siblings )
            {
                if( siblingNode.Children != null )
                {
                    return; // one of the siblings is subdivided
                }
            }

            Vector2 center = q.Node.Parent.Center;
            Vector3 origin = q._face.GetSpherePoint( center.x, center.y ) * (float)q.CelestialBody.Radius;
            int lN = q.SubdivisionLevel - 1;

            LODQuad quad = Create( q.transform.parent, origin, q._quadSphere, q.CelestialBody, q.EdgeSubdivisions, center, lN, q.Node.Parent, q.SubdivisionDistance * 2f, q._face );
            quad.airfPOI = q.airfPOI; // IMPORTANT.

            // Destroy the other 3 siblings of the unsubdivided quad.
            foreach( var qSibling in q.Node.Parent.Children )
            {
                qSibling.Value.airfPOI = null;
                Destroy( qSibling.Value.gameObject );
            }

            q.Node.Parent.Children = null;
        }

        /// <summary>
        /// Creates and caches the mesh for this LODQuad.
        /// </summary>
        void GenerateMeshData()
        {
            MakeQuadMesh_Job meshJob = new MakeQuadMesh_Job()
            {
                subdivisions = EdgeSubdivisions,
                radius = (float)CelestialBody.Radius,
                center = this.Node.Center,
                lN = SubdivisionLevel,
                origin = this.transform.localPosition
            };

            meshJob.Initialize();

#warning TODO - add a proper state management to the LODQuad and make it schedule everything in update, wait for completion in lateupdate, depending on state. Currently the job doesn't actually speed up anything because of the waiting.
            JobHandle jobHandle = meshJob.Schedule();

            jobHandle.Complete();

            Mesh mesh = new Mesh();

            mesh.SetVertices( meshJob.vertices );
            mesh.SetNormals( meshJob.normals );
            mesh.SetUVs( 0, meshJob.uvs );
            mesh.SetTriangles( meshJob.triangles.ToArray(), 0 );
            // tangents calc'd here because job can't create Mesh object to calc them.
            mesh.RecalculateTangents();
            mesh.FixTangents(); // fix broken tangents.
            mesh.RecalculateBounds();

            this._meshCollider.sharedMesh = mesh;
            this._meshFilter.sharedMesh = mesh;

            meshJob.vertices.Dispose();
            meshJob.normals.Dispose();
            meshJob.uvs.Dispose();
            meshJob.triangles.Dispose();
        }

        public static LODQuad Create( Transform parent, Vector3 localPosition, LODQuadSphere quadSphere, CelestialBody celestialBody, int edgeSubdivisions, Vector2 center, int lN, LODQuadTree.Node node, float subdivisionDistance, QuadSphereFace face )
        {
            // FIXME: this method kinda ugly. And prevent people from the outside from being able to create non-l0 quads.

            GameObject go = new GameObject( $"LODQuad L{lN}, {face}, ({center.x:#0.################}, {center.y:#0.################})" );
            go.transform.SetParent( parent );

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = FindObjectOfType<zzzTestGameManager>().CBMaterial;

            go.AddComponent<MeshCollider>();

            LODQuad q = go.AddComponent<LODQuad>();
            q.Node = node;
            node.Value = q;

            if( lN > 0 )
            {
                (int x, int y) = LODQuadUtils.GetChildIndex( q.Node.Parent.Center, center );

                q.Node.Parent.Children[x, y].Value = q;
            }

            q.CelestialBody = celestialBody;
            q._quadSphere = quadSphere;
            q.SubdivisionDistance = subdivisionDistance;

            q.EdgeSubdivisions = edgeSubdivisions;
            q.SubdivisionLevel = lN;
            q._face = face;

            // Unity keeps the local positions of objects internally.
            q.transform.localPosition = localPosition;
            q.transform.localRotation = Quaternion.identity;
            q.transform.localScale = Vector3.one;

            q.GenerateMeshData();

            return q;
        }
    }
}