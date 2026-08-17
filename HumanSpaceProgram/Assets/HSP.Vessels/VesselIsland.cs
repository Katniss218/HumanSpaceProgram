using System.Collections.Generic;
using System.Linq;

namespace HSP.Vessels
{
    public class VesselIsland : IReadonlyVesselIsland
    {
        private List<VesselPart> _parts;
        private readonly FComponentCache _componentCache = new FComponentCache();

        public VesselPart[] Parts => _parts.ToArray();
        IReadOnlyList<VesselPart> IReadonlyVesselIsland.Parts => _parts;

        public VesselIsland()
        {
            _parts = new List<VesselPart>();
        }

        public VesselIsland( IEnumerable<VesselPart> parts )
        {
            _parts = parts.ToList();
            RebuildCache();
        }

        public void AddPart( VesselPart part )
        {
            if( !_parts.Contains( part ) )
            {
                _parts.Add( part );
                _componentCache.AddRange( part.Components );
            }
        }

        public void RemovePart( VesselPart part )
        {
            if( _parts.Remove( part ) )
            {
                _componentCache.RemoveRange( part.Components );
            }
        }

        private void RebuildCache()
        {
            _componentCache.Clear();
            foreach( var part in _parts )
            {
                if( part != null )
                {
                    _componentCache.AddRange( part.Components );
                }
            }
        }

        public IReadOnlyList<T> GetFComponents<T>() where T : class
        {
            return _componentCache.Get<T>();
        }
    }
}