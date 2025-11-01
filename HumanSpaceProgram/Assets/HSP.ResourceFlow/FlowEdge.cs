using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public struct FlowEdge
    {
        public FlowNode end1;
        public FlowNode end2;

        public double Volume { get; set; }

        public FlowEdge( FlowNode end1, FlowNode end2 )
        {
            this.end1 = end1;
            this.end2 = end2;
        }

        public float GetLength()
        {
            return Vector3.Distance( end1.pos, end2.pos );
        }
    }
}