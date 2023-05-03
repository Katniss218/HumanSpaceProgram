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

        static readonly float Cos45DegPlusEpsilon = Mathf.Cos( 45 * Mathf.Deg2Rad ) + 0.025f; // also, cos(45deg) is equal to sin(45deg)

        public static void UpdateNeighbors( IEnumerable<LODQuad> neighborQuads, LODQuad newQuad, int direction )
        {
            List<LODQuad> neighborsInSpecifiedDirection = new List<LODQuad>();

            // Find all nodes that lay in a given direction from the current node.
            foreach( var potentialQuad in neighborQuads )
            {
                if( potentialQuad == newQuad )
                    continue;

                Vector2 toNode = potentialQuad.Node.Center - newQuad.Node.Center;

                if( toNode == Vector2.zero )
                    continue;

                toNode.Normalize();

                // if vector's principal axis points towards direction.
                float dot = Vector2.Dot( toNode, Directions[direction] );
                if( dot > Cos45DegPlusEpsilon )
                {
                    neighborsInSpecifiedDirection.Add( potentialQuad );
                }
            }

            int inverseDirection = InverseDir[direction];

            // Always update the neighbors' edges - in case the current node was unsubdivided, and the edges of all its contacting neighbors need to be updated.
            foreach( var neighbor in neighborsInSpecifiedDirection )
            {
                if( newQuad.SubdivisionLevel > neighbor.SubdivisionLevel ) // equivalent to checking whether there are multiple quads contacting the neighbor.
                {
                    continue;
                }

                neighbor._edges[inverseDirection] = newQuad;
            }

            if( neighborsInSpecifiedDirection.Count != 1 )
            {
                // Don't update the current node if there are multiple (smaller) nodes contacting it from a given direction.
                // - Otherwise, possibly `unsubdividedQuad.neighbors[direction] = mixed;`. Maybe mark the edge with highest, lowest, or maybe mark each interval. idk.
                return;
            }

            newQuad._edges[direction] = neighborsInSpecifiedDirection[0];
        }

        public static int[] GetOuterDirections( Vector2 dir )
        {
            // return 2 outer directions for the corresponding vector.
            // 0 - x-, 1 - x+, 2 - y-, 3 - y+

            int[] r = new int[2];

            if( dir.x < 0 )
                r[0] = 0;
            else
                r[0] = 1;

            if( dir.y < 0 )
                r[1] = 2;
            else
                r[1] = 3;

            return r;
        }

        public static int[] GetOppositeDirs( int[] dir )
        {
            List<int> r = new List<int>();
            if( !dir.Contains( 0 ) )
                r.Add( 0 );
            if( !dir.Contains( 1 ) )
                r.Add( 1 );
            if( !dir.Contains( 2 ) )
                r.Add( 2 );
            if( !dir.Contains( 3 ) )
                r.Add( 3 );
            return r.ToArray();
        }

        // move this somewhere, potentially add an enum to represent directions.
        public static Vector2[] Directions = new Vector2[4]
        {
            new Vector2( -1, 0 ),
            new Vector2( 1, 0 ),
            new Vector2( 0, -1 ),
            new Vector2( 0, 1 ),
        };

        public static int[] InverseDir = new int[4]
        {
            1,0,3,2
        };

        /// <summary>
        /// Returns the axis vector for the axis with the largest component. The main axis of a vector.
        /// </summary>
        public static Vector2 GetPrincipalAxis( Vector2 v )
        {
            int indexToClear;

            if( Math.Abs( v.y ) > Math.Abs( v.x ) )
            {
                indexToClear = 0;
            }
            else
            {
                indexToClear = 1;
            }

            v[indexToClear] = 0.0f;
            return v;
        }


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

            var rootNode = q.Node.Root;

            // Destroy the big quad.
            Destroy( q.gameObject );
            q.Node.Value = null;
            // Don't make the parent a leaf, because once subdivided there definitely are children.

            int subdividedLN = q.SubdivisionLevel + 1;

            LODQuad[] _4_quads = new LODQuad[4];

            for( int i = 0; i < 4; i++ )
            {
                (int xIndex, int yIndex) = LODQuadTree_NodeUtils.GetChildIndex( i );

                Vector2 subdividedCenter = LODQuadTree_NodeUtils.GetChildCenter( q.Node, xIndex, yIndex );
                Vector3 subdividedOrigin = q._face.GetSpherePoint( subdividedCenter.x, subdividedCenter.y ) * (float)q.CelestialBody.Radius;

                LODQuadTree.Node newNode = new LODQuadTree.Node( q.Node, subdividedCenter, LODQuadTree_NodeUtils.GetSize( subdividedLN ) );

                LODQuad newQuad = Create( q.transform.parent, subdividedOrigin, q._quadSphere, q.CelestialBody, subdividedCenter, subdividedLN, newNode, q.SubdivisionDistance / 2f, q._meshRenderer.material, q._face );
                _4_quads[i] = newQuad;
            }

            foreach( var quad in _4_quads )
            {
#warning TODO - Add a way to include other faces in the query. And not just the quad face of the current quad. Combine the 6 quadtrees into one datastructure.

                // Query area of each node separately
                // - because if the entire area is queried, then the nodes that are not direct neighbors of the current node are included and it breaks shit.
                List<LODQuadTree.Node> queryResult = rootNode.QueryLeafNodes( quad.Node.minX, quad.Node.minY, quad.Node.maxX, quad.Node.maxY );

                Vector2 toQuad = quad.Node.Center - q.Node.Center;

                // update inside edges too.
                int[] fourDirs = new int[4] { 0, 1, 2, 3 };

                foreach( var direction in fourDirs )
                {
                    UpdateNeighbors( queryResult.Where( n => n.Value != null ).Select( n => n.Value ), quad, direction );
                }
            }
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

            // cache before removing the node from the quadtree.
            var rootNode = q.Node.Root;
            var parentNode = q.Node.Parent;

            // Destroy the old quads that were replaced by the unsubdivided quad.
            foreach( var qSibling in q.Node.Siblings )
            {
                Destroy( qSibling.Value.gameObject );
                qSibling.Value = null;
            }
            q.Node.Parent.MakeLeafNode(); // make the parent a leaf, because once unsubdivided, there will be no children.
                                          // Also should prevent its inclusion in the query.

            // create new node.
            Vector2 unsubdividedCenter = parentNode.Center;
            Vector3 unsubdividedOrigin = q._face.GetSpherePoint( unsubdividedCenter.x, unsubdividedCenter.y ) * (float)q.CelestialBody.Radius;
            int unsubdividedLN = q.SubdivisionLevel - 1;

            LODQuad newQuad = Create( q.transform.parent, unsubdividedOrigin, q._quadSphere, q.CelestialBody, unsubdividedCenter, unsubdividedLN, parentNode, q.SubdivisionDistance * 2f, q._meshRenderer.material, q._face );

            List<LODQuadTree.Node> queryResult = rootNode.QueryLeafNodes( newQuad.Node.minX, newQuad.Node.minY, newQuad.Node.maxX, newQuad.Node.maxY );

            // update edges.
            int[] all_4_directions = new int[4] { 0, 1, 2, 3 };

            foreach( var direction in all_4_directions )
            {
                UpdateNeighbors( queryResult.Where( n => n.Value != null ).Select( n => n.Value ), newQuad, direction );
            }
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