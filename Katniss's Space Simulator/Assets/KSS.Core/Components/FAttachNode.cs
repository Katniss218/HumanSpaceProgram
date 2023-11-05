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
        public FAttachNode TrySnap( Transform obj, IEnumerable<FAttachNode> other )
        {
            var orderedNodes = other.OrderByDescending( o => o.Range );

            Vector3 pos = this.transform.position;
            Vector3 forward = this.transform.forward;

            foreach( var node in orderedNodes )
            {
                if( Vector3.Distance( pos, node.transform.position ) < Mathf.Max( this.Range, node.Range ) )
                {
                    if( Vector3.Angle( forward, node.transform.forward ) > (180 - SnapThresholdAngle) )
                    {
                        this.SnapTo( obj, node );
                        return node;
                    }
                }
            }
#warning TODO - ioncorporate view direction and position (view ray).

            return null;
        }

        public void SnapTo( Transform obj, FAttachNode other )
        {
            Quaternion nodeRotation = Quaternion.FromToRotation( -this.transform.forward, other.transform.forward );
            obj.transform.rotation = nodeRotation * obj.transform.rotation;

            nodeRotation = Quaternion.FromToRotation( this.transform.up, other.transform.up );
            obj.transform.rotation = nodeRotation * obj.transform.rotation;

            Vector3 offset = obj.transform.position - this.transform.position;
            obj.position = other.transform.position + offset;
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