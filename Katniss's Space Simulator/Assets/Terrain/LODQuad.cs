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
        /// <summary>
        /// Describes what a given <see cref="LODQuad"/> is currently doing.
        /// </summary>
        public enum State
        {
            Idle, // do nothing.
            Active, // check for subdivs.
            GeneratingMesh
        }

        // Generated meshes have relatively higher precision because the vertices are closer to the origin of the mesh, than to the origin of the celestial body.

        public State CurrentState { get; private set; }

        public LODQuadTree.Node Node { get; private set; }

        /// <summary>
        /// How many edge subdivision steps to apply for each quad, on top of and regardless of <see cref="SubdivisionLevel"/>.
        /// </summary>
        public int EdgeSubdivisions { get => _quadSphere.EdgeSubdivisions; }

        public CelestialBody CelestialBody { get; private set; }

        public float SubdivisionDistance { get; private set; }

        QuadSphereFace _face;

        MeshFilter _meshFilter;
        MeshCollider _meshCollider;
        MeshRenderer _meshRenderer;

        public Vector3Dbl[] airfPOIs { get; set; }

        [field: SerializeField]
        public int SubdivisionLevel { get; private set; }

        LODQuadSphere _quadSphere;

        MakeQuadMesh_Job _job;
        JobHandle _jobHandle;

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshCollider = this.GetComponent<MeshCollider>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if( CurrentState == State.Idle )
            {
                return;
            }

            if( CurrentState == State.Active )
            {
                if( ShouldSubdivide() )
                {
                    Subdivide( this );
                    return;
                }

                if( ShouldUnsubdivide() )
                {
                    Unsubdivide( this );
                    return;
                }
            }
        }

        private void LateUpdate()
        {
            if( CurrentState == State.GeneratingMesh )
            {
                _jobHandle.Complete();

                _job.Finish(this);

                this.CurrentState = State.Active;
            }
        }

        internal void SetMesh( Mesh mesh )
        {
            this._meshCollider.sharedMesh = mesh;
            this._meshFilter.sharedMesh = mesh;
        }

        private bool ShouldSubdivide()
        {
            if( this.SubdivisionLevel >= _quadSphere.HardLimitSubdivLevel )
            {
                return false;
            }

            if( airfPOIs == null )
            {
                return false;
            }

            if( airfPOIs.Length == 0 )
            {
                return false;
            }

            // Check if any of the PIOs is within the subdiv radius.
            foreach( var airfPOI in this.airfPOIs )
            {
                Vector3Dbl airfQuad = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.transform.position );
                double dist = (airfPOI - airfQuad).magnitude;

                if( (float)dist < SubdivisionDistance )
                {
                    return true;
                }
            }
            return false;
        }

        private bool ShouldUnsubdivide()
        {
            if( this.SubdivisionLevel == 0 )
            {
                return false;
            }

            if( airfPOIs == null )
            {
                return false;
            }

            if( airfPOIs.Length == 0 )
            {
                return true;
            }

            foreach( var siblingNode in this.Node.Siblings )
            {
                // Don't unsubdivide if one of the siblings is subdivided. That would require handling nested unsubdivisions, and is nasty, blergh and unnecessary.
                if( siblingNode.Children != null )
                {
                    return false;
                }
                // Sibling node is still generating - don't unsubdivide.
                // Not having this can lead to memory leaks with jobs.
                if( siblingNode.Value != null && siblingNode.Value.CurrentState == State.GeneratingMesh )
                {
                    return false;
                }
            }

            // If the parent won't want to immediately subdivide again, unsubdivide.
            Vector3 originBodySpace = _face.GetSpherePoint( this.Node.Parent.Center ) * (float)CelestialBody.Radius;
            foreach( var airfPOI in this.airfPOIs )
            {
                Vector3Dbl parentQuadOriginAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.CelestialBody.transform.TransformPoint( originBodySpace ) );
                double distanceToPoi = (airfPOI - parentQuadOriginAirf).magnitude;

                if( distanceToPoi > this.SubdivisionDistance * 2 ) // times 2 because parent subdiv range is 2x more than its child.
                {
                    return true;
                }
            }

            return false;
        }

#warning TODO - LOD Terrain edge interpolation.
        // we need something to tell the connectivity of parts. Could use the quadtree.

        /// <summary>
        /// Splits the specified quad into 4 separate quads.
        /// </summary>
        private static void Subdivide( LODQuad q )
        {
            if( q.Node.Children != null )
            {
                throw new InvalidOperationException( "Tried subdividing a subdivided node" );
            }

            if( q.SubdivisionLevel >= q._quadSphere.HardLimitSubdivLevel )
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

                LODQuad quad = Create( q.transform.parent, origin, q._quadSphere, q.CelestialBody, center, lN, q.Node.Children[xIndex, yIndex], q.SubdivisionDistance / 2f, q._meshRenderer.material, q._face );
            }

            q.airfPOIs = null;
            q.Node.Value = null;

            Destroy( q.gameObject );
        }

        /// <summary>
        /// Joins the specified quad and its 3 siblings into a single quad of lower level.
        /// </summary>
        private static void Unsubdivide( LODQuad q )
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

            LODQuad quad = Create( q.transform.parent, origin, q._quadSphere, q.CelestialBody, center, lN, q.Node.Parent, q.SubdivisionDistance * 2f, q._meshRenderer.material, q._face );
            quad.airfPOIs = q.airfPOIs; // IMPORTANT.

            // Destroy the other 3 siblings of the unsubdivided quad.
            foreach( var qSibling in q.Node.Parent.Children )
            {
                //qSibling.Value.airfPOIs = null;
                Destroy( qSibling.Value.gameObject );
            }

            q.Node.Parent.Children = null;
        }

        /// <summary>
        /// Creates and caches the mesh for this LODQuad.
        /// </summary>
        private void GenerateMeshData()
        {
            this.CurrentState = State.GeneratingMesh;

            _job = new MakeQuadMesh_Job();

            _job.Initialize( this ); // This (and collection) would have to be Reflection-ified to make it extendable by other user-provided assemblies.
            _jobHandle = _job.Schedule();
        }

        public static LODQuad Create( Transform parent, Vector3 localPosition, LODQuadSphere quadSphere, CelestialBody celestialBody, Vector2 center, int lN, LODQuadTree.Node node, float subdivisionDistance, Material mat, QuadSphereFace face )
        {
            // FIXME: this method kinda ugly. And prevent people from the outside from being able to create non-l0 quads.

            GameObject go = new GameObject( $"LODQuad L{lN}, {face}, ({center.x:#0.################}, {center.y:#0.################})" );
            go.transform.SetParent( parent );

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;

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

            q.SubdivisionLevel = lN;
            q._face = face;

            // Unity keeps the local positions of objects internally.
            q.transform.localPosition = localPosition;
            q.transform.localRotation = Quaternion.identity;
            q.transform.localScale = Vector3.one;
            q.CurrentState = State.Idle;

            q.GenerateMeshData();

            return q;
        }
    }
}