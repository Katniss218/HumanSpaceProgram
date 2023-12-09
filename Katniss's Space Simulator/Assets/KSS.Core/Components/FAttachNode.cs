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
    public sealed class FAttachNode : MonoBehaviour, IPersistent
    {
        // An attachment node is supposed to be placed on its own gameobject. 

        /// <summary>
        /// The distance at which this node will snap with other nodes.
        /// </summary>
        /// <remarks>
        /// Note that snapping takes the max(n1, n2).
        /// </remarks>
        [field: SerializeField]
        public float Range { get; set; }

        const float SnapThresholdAngle = 45;

        public struct SnappingCandidate
        {
            public FAttachNode snappedNode;
            public FAttachNode targetNode;
            public float distance;
            public float angle;

            public SnappingCandidate( FAttachNode snappedNode, FAttachNode targetNode, float distance, float angle )
            {
                this.snappedNode = snappedNode;
                this.targetNode = targetNode;
                this.distance = distance;
                this.angle = angle;
            }
        }

        /// <summary>
        /// Tries to snap the node to any of the specified nodes. Takes the snapping rules into account.
        /// </summary>
        public static SnappingCandidate? GetSnappingNodePair( FAttachNode[] snappedNodes, FAttachNode[] targetNodes, Vector3 viewDirection )
        {
            List<SnappingCandidate> nodePairs = new List<SnappingCandidate>();
            foreach( var objNode in snappedNodes )
            {
                foreach( var targetNode in targetNodes )
                {
                    Vector3 projectedObjNode = Vector3.ProjectOnPlane( objNode.transform.position, viewDirection );
                    Vector3 projectedTargetNode = Vector3.ProjectOnPlane( targetNode.transform.position, viewDirection );
                    float distance = Vector3.Distance( projectedObjNode, projectedTargetNode );
                    float angle = Vector3.Angle( -objNode.transform.forward, targetNode.transform.forward );

                    if( distance > Mathf.Max( objNode.Range, targetNode.Range )
                     || angle > SnapThresholdAngle )
                    {
                        continue;
                    }

                    nodePairs.Add( new SnappingCandidate( objNode, targetNode, distance, angle ) );
                }
            }

            List<SnappingCandidate> orderedNodePairs = nodePairs.OrderBy( n => n.distance ).ToList();

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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( this.transform.position, 0.125f );
            Gizmos.DrawWireSphere( this.transform.position, this.Range / 2 );
            Gizmos.DrawLine( this.transform.position, this.transform.position + this.transform.forward * this.Range );
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "range", this.Range }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "range", out var range ) )
                this.Range = (float)range;
        }
    }
}