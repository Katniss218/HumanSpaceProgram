using System.Collections.Generic;
using System.Linq;

namespace HSP.Vessels
{
    public class VesselIsland : IReadonlyVesselIsland
    {
        private List<VesselPart> _parts;

        public VesselPart[] Parts => _parts.ToArray();
        IReadOnlyList<VesselPart> IReadonlyVesselIsland.Parts => _parts;

        public VesselIsland()
        {
            _parts = new List<VesselPart>();
        }

        public VesselIsland( IEnumerable<VesselPart> parts )
        {
            _parts = parts.ToList();
        }

        public void AddPart( VesselPart part )
        {
            if( !_parts.Contains( part ) )
            {
                _parts.Add( part );
            }
        }
    }
}