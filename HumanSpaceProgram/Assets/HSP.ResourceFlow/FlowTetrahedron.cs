using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.ResourceFlow
{
    public sealed class FlowTetrahedron
    {
        public readonly FlowNode v0;
        public readonly FlowNode v1;
        public readonly FlowNode v2;
        public readonly FlowNode v3;

        public FlowTetrahedron( FlowNode v0, FlowNode v1, FlowNode v2, FlowNode v3 )
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
}