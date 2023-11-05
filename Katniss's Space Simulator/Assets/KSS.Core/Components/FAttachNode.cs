using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core.Components
{
    public class FAttachNode : MonoBehaviour, IPersistent
    {
        [field: SerializeField]
        public float Range { get; set; }

        // nodes don't handle reparenting, or other. they only position things.

        const float SnapThresholdAngle = 45;

        /// <summary>
        /// Tries to snap the node to any of the specified nodes. Takes the snapping rules into account.
        /// </summary>
        public static (FAttachNode obj, FAttachNode tgt)? TrySnap( Transform obj, FAttachNode[] objNodes, FAttachNode[] targetNodes )
        {
            List<(FAttachNode obj, FAttachNode tgt)> nodePairs = new List<(FAttachNode, FAttachNode)>();
            foreach( var objNode in objNodes )
            {
                foreach( var targetNode in targetNodes )
                {
#warning TODO - incorporate view ray into the filtering.
                    if( Vector3.Distance( objNode.transform.position, targetNode.transform.position ) > Mathf.Max( objNode.Range, targetNode.Range )
                     || Vector3.Angle( -objNode.transform.forward, targetNode.transform.forward ) > SnapThresholdAngle )
                    {
                        continue;
                    }

                    nodePairs.Add( (objNode, targetNode) );
                }
            }

            var orderedNodePairs = nodePairs.OrderBy( tuple => Vector3.Distance( tuple.obj.transform.position, tuple.tgt.transform.position ) ).ToList();

            var tuple = orderedNodePairs.FirstOrDefault();
            if( tuple != default )
            {
                SnapTo( obj, tuple.obj, tuple.tgt );
                return tuple;
            }

            return null;
        }

        public static void SnapTo( Transform obj, FAttachNode objNode, FAttachNode targetNode )
        {
            Quaternion nodeRotation = Quaternion.FromToRotation( -objNode.transform.forward, targetNode.transform.forward );
            obj.transform.rotation = nodeRotation * obj.transform.rotation;

            nodeRotation = Quaternion.FromToRotation( objNode.transform.up, targetNode.transform.up );
            obj.transform.rotation = nodeRotation * obj.transform.rotation;

            Vector3 offset = obj.transform.position - objNode.transform.position;
            obj.position = targetNode.transform.position + offset;
        }

        // A node is supposed to be placed on its own gameobject. the orientation of that 

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( this.transform.position, 0.125f );
            Gizmos.DrawWireSphere( this.transform.position, this.Range / 2 );
            Gizmos.DrawLine( this.transform.position, this.transform.position + this.transform.forward * this.Range );
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "range", this.Range }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "range", out var range ) )
                this.Range = (float)range;
        }
    }
}