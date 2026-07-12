using System.Collections.Generic;
using System.Linq;

namespace HSP.Vessels
{
    public class VesselIsland
    {
        public VesselPart[] Parts { get; }

        public VesselIsland( IEnumerable<VesselPart> parts )
        {
            Parts = parts.ToArray();
        }
    }
}