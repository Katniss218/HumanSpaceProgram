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

        [SerializeField]
        LODQuad[] _edges = new LODQuad[4];

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

                _job.Finish( this );

                this.CurrentState = State.Active;
            }
        }

        internal void SetMesh( Mesh mesh )
        {
            this._meshCollider.sharedMesh = mesh; // this is kinda slow.
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

            if( airfPOIs == null ) // default.
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

        // Quads with lN1 adjust to the neighboring quads with lN2 <= lN1
        // I.e. smaller quads adjust to bigger quads.

        // the smaller quads will have their start/end vertices aligned with vertices on the larger quads if the contacting meshes don't have too different subdiv count

        // the edge subdivision count determines the maximum difference in subdiv level of adjacent quads that will still align the start/end vertices on the small one.


        // any height shaping function shouldn't change its output value for the same position, regardless of the quad calling it.

        // ### 2. option 2

        // we query the quadtree for the rectangle defined by the node that is being subdivided? (using <= instead of < we don't have to make it bigger because exactly representable floats)
        // - this is simpler, less things to sync up, and would probably perform roughly the same in terms of speed.
        // if min/max of the query region is 0 or 2, we can also query the edge of the adjacent quadtrees of the other sphere faces.
        // maybe represent that as a separate class that can handle that.

        // quadtree can be modified to store its min/max coordinates directly, with size/center accessors.
        // since the subdivisions are binary, starting at size=2, these points will always be representable exactly (until we run out of available space in the exponent of the float).

        // querying the quadtree for that should be reasonably fast.


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

            // Leaf Nodes that border the node `q`, as well as `q` itself is also included.
            var queriedNeighborsAndSelf = q.Node.Root.QueryLeafNodes( q.Node.minX, q.Node.minY, q.Node.maxX, q.Node.maxY );

            int subdividedLN = q.SubdivisionLevel + 1;

            for( int i = 0; i < 4; i++ )
            {
                (int xIndex, int yIndex) = LODQuadTree_NodeUtils.GetChildIndex( i );

                Vector2 subdividedCenter = LODQuadTree_NodeUtils.GetChildCenter( q.Node, xIndex, yIndex );
                Vector3 subdividedOrigin = q._face.GetSpherePoint( subdividedCenter.x, subdividedCenter.y ) * (float)q.CelestialBody.Radius;

                LODQuadTree.Node newNode = new LODQuadTree.Node( q.Node, subdividedCenter, LODQuadTree_NodeUtils.GetSize( subdividedLN ) );

                LODQuad newQuad = Create( q.transform.parent, subdividedOrigin, q._quadSphere, q.CelestialBody, subdividedCenter, subdividedLN, newNode, q.SubdivisionDistance / 2f, q._meshRenderer.material, q._face );

#warning TODO - We also need to set the edges for the newly subdivided nodes.
                // Those will in most cases almost for sure contact a larger node on at least one side.

                // if the new node should be interpolated, then it will have at most one neighbor in a given direction, because of how they are subdivided.
                foreach( var node in queriedNeighborsAndSelf )
                {
                    // since there is at most 1 node, we can just loop over them, discard any that are smaller
                }
                // 
                //quad._edges = q._edges; // not accurate for internal edges, only external
            }

            // Update the surrounding nodes.
            // this part is only really important for updating nodes that once had a larger neighbor, but no longer have one because it was subdivided.
            foreach( var node in queriedNeighborsAndSelf )
            {
                if( q.Node == node )
                    continue; // itself.

                if( node.Size > q.Node.Size )
                    continue; // we don't want to update nodes that are larger than us.

                Vector2 dir = q.Node.Center - node.Center;

                if( Mathf.Abs( dir.x ) == Mathf.Abs( dir.y ) )
                    continue; // corner node.

                // We have to set the edges of the queried nodes to whatever the subdivided nodes are, but ONLY IF if the new subdivided nodes are smaller than the queried nodes.
                // do the opposite when unsubdivving.

                //int index = LODQuadTree_NodeUtils.GetEdgeIndex( dir );
                //node.Value._edges[index] = q;
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

            foreach( var qSibling in q.Node.Siblings )
            {
                if( qSibling.Children != null )
                {
                    return; // one of the siblings is subdivided
                }
            }

            Vector2 unsubdividedCenter = q.Node.Parent.Center;
            Vector3 unsubdividedOrigin = q._face.GetSpherePoint( unsubdividedCenter.x, unsubdividedCenter.y ) * (float)q.CelestialBody.Radius;
            int unsubdividedLN = q.SubdivisionLevel - 1;

            LODQuad newQuad = Create( q.transform.parent, unsubdividedOrigin, q._quadSphere, q.CelestialBody, unsubdividedCenter, unsubdividedLN, q.Node.Parent, q.SubdivisionDistance * 2f, q._meshRenderer.material, q._face );

            // Destroy the old quads that were replaced by the unsubdivided quad.
            foreach( var qSibling in q.Node.Siblings )
            {
                Destroy( qSibling.Value.gameObject );
            }

            q.Node.Parent.MakeLeafNode();
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

            LODQuad newQuad = go.AddComponent<LODQuad>();
            newQuad.Node = node;
            node.Value = newQuad;

            if( lN > 0 )
            {
                (int x, int y) = LODQuadTree_NodeUtils.GetChildIndex( newQuad.Node );

                newQuad.Node.Parent.Children[x, y].Value = newQuad;
            }

            newQuad.CelestialBody = celestialBody;
            newQuad._quadSphere = quadSphere;
            newQuad.SubdivisionDistance = subdivisionDistance;

            newQuad.SubdivisionLevel = lN;
            newQuad._face = face;

            // Unity keeps the local positions of objects internally.
            newQuad.transform.localPosition = localPosition;
            newQuad.transform.localRotation = Quaternion.identity;
            newQuad.transform.localScale = Vector3.one;
            newQuad.CurrentState = State.Idle;

            newQuad.GenerateMeshData();

            return newQuad;
        }
    }
}