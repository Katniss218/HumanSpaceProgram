using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core.Components
{
    [DisallowMultipleComponent]
    public sealed class FAttachNode : MonoBehaviour, IPersistsData
    {
        /// <summary>
        /// A struct representing a candidate node pair that can be used for snapping.
        /// </summary>
        public struct SnappingCandidate
        {
            /// <summary>
            /// The node that was snapped to the target.
            /// </summary>
            public FAttachNode snappedNode;
            /// <summary>
            /// The node that the snapped node was snapped to.
            /// </summary>
            public FAttachNode targetNode;
            public float distance;
            public float angle;

            /// <summary>
            /// Describes which node pair should take priority when trying to snap.
            /// </summary>
            public float PriorityScore => distance;

            public SnappingCandidate( FAttachNode snappedNode, FAttachNode targetNode, float distance, float angle )
            {
                this.snappedNode = snappedNode;
                this.targetNode = targetNode;
                this.distance = distance;
                this.angle = angle;
            }
        }

        // An attachment node is supposed to be placed on its own empty gameobject.

        /// <summary>
        /// The distance at which this node will snap with other nodes.
        /// </summary>
        /// <remarks>
        /// Note that snapping uses max(n1, n2).
        /// </remarks>
        [field: SerializeField]
        public float Range { get; set; }

        const float SnapThresholdAngle = 45;

        // attach nodes of different types could use different meshes.
        // These meshes should be separate from the node object itself, and can be pooled.
        // They can also be dynamically hidden/shown based on which nodes are enabled/used/etc.

        // attach nodes have a separate global map that specifies which node types can connect to which types (as either a whitelist or a blacklist)
        public string NodeType { get; set; }

        /// <summary>
        /// Figures out which node pair is the best candidate for snapping.
        /// </summary>
        public static SnappingCandidate? GetBestSnappingNodePair( FAttachNode[] snappedNodes, FAttachNode[] targetNodes, Vector3 viewDirection )
        {
            List<SnappingCandidate> nodePairs = new List<SnappingCandidate>();
            foreach( var objNode in snappedNodes )
            {
                foreach( var targetNode in targetNodes )
                {
                    float angle = Vector3.Angle( -objNode.transform.forward, targetNode.transform.forward );
                    if( angle > SnapThresholdAngle )
                    {
                        continue;
                    }

                    Vector3 projectedObjNode = Vector3.ProjectOnPlane( objNode.transform.position, viewDirection );
                    Vector3 projectedTargetNode = Vector3.ProjectOnPlane( targetNode.transform.position, viewDirection );
                    float distance = Vector3.Distance( projectedObjNode, projectedTargetNode );
                    if( distance > Mathf.Max( objNode.Range, targetNode.Range ) )
                    {
                        continue;
                    }

                    nodePairs.Add( new SnappingCandidate( objNode, targetNode, distance, angle ) );
                }
            }

            List<SnappingCandidate> orderedNodePairs = nodePairs.OrderBy( n => n.PriorityScore ).ToList();

            if( nodePairs.Count < 1 )
            {
                return null;
            }

            return orderedNodePairs[0];
        }

        /// <summary>
        /// Translates and rotates the <paramref name="snappedObject"/> to align the 2 nodes.
        /// </summary>
        /// <remarks>
        /// After snapping: <br/>
        /// - <paramref name="snappedNode"/>'s world position is equal to <paramref name="targetNode"/>'s world position. <br/>
        /// - <paramref name="snappedNode"/>'s world rotation is opposite to <paramref name="targetNode"/>'s world rotation.
        /// </remarks>
        public static void SnapTo( Transform snappedObject, FAttachNode snappedNode, FAttachNode targetNode )
        {
            if( !snappedNode.transform.IsChildOf( snappedObject ) )
            {
                throw new ArgumentException( $"The snapped node must be a child of the snapped object." );
            }

            // Align the 'up' directions.
            Quaternion rotation = Quaternion.FromToRotation( snappedNode.transform.up, targetNode.transform.up );
            snappedObject.transform.rotation = rotation * snappedObject.transform.rotation;

            // Align the 'forward' directions. This should be the last rotation, to make absolute sure that forward is always matching.
            rotation = Quaternion.FromToRotation( snappedNode.transform.forward, -targetNode.transform.forward );
            snappedObject.transform.rotation = rotation * snappedObject.transform.rotation;

            // Match positions.
            Vector3 offset = snappedObject.transform.position - snappedNode.transform.position;
            snappedObject.position = targetNode.transform.position + offset;
        }
        
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "range", this.Range }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "range", out var range ) )
                this.Range = (float)range;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( this.transform.position, 0.125f );
            Gizmos.DrawWireSphere( this.transform.position, this.Range / 2 );
            Gizmos.DrawLine( this.transform.position, this.transform.position + this.transform.forward * this.Range );
        }
    }
}