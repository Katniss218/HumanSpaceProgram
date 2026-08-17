using System;
using System.Collections.Generic;

namespace HSP.Vessels
{
    /// <summary>
    /// Provides an efficient cache for retrieving FComponents by type or interface.
    /// Invalidate the cache by adding, removing, or clearing components.
    /// </summary>
    public class FComponentCache
    {
        private readonly List<FComponent> _components = new List<FComponent>();
        private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public IEnumerable<FComponent> AllComponents => _components;

        public void Add( FComponent component )
        {
            if( component == null ) return;
            if( !_components.Contains( component ) )
            {
                _components.Add( component );
                _cache.Clear();
            }
        }

        public void AddRange( IEnumerable<FComponent> components )
        {
            if( components == null ) return;
            bool added = false;
            foreach( var c in components )
            {
                if( c != null && !_components.Contains( c ) )
                {
                    _components.Add( c );
                    added = true;
                }
            }
            if( added )
            {
                _cache.Clear();
            }
        }

        public void Remove( FComponent component )
        {
            if( component == null ) return;
            if( _components.Remove( component ) )
            {
                _cache.Clear();
            }
        }

        public void RemoveRange( IEnumerable<FComponent> components )
        {
            if( components == null ) return;
            bool removed = false;
            foreach( var c in components )
            {
                if( c != null && _components.Remove( c ) )
                {
                    removed = true;
                }
            }
            if( removed )
            {
                _cache.Clear();
            }
        }

        public void Clear()
        {
            if( _components.Count > 0 )
            {
                _components.Clear();
                _cache.Clear();
            }
        }

        public IReadOnlyList<T> Get<T>() where T : class
        {
            Type type = typeof( T );
            if( !_cache.TryGetValue( type, out object listObj ) )
            {
                var list = new List<T>();
                foreach( var comp in _components )
                {
                    if( comp is T typedComp )
                    {
                        list.Add( typedComp );
                    }
                }
                listObj = list;
                _cache[type] = listObj;
            }

            return (IReadOnlyList<T>)listObj;
        }
    }
}
