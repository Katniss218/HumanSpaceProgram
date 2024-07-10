using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public class MapsAnySearcher<TContext, T> : IMappingProviderSearcher<TContext, T>
    {
        private readonly Dictionary<TContext, T> _map = new();

        public MapsAnySearcher()
        {

        }

        public bool TryGet( TContext context, Type type, out T value )
        {
            if( type == null )
            {
                throw new ArgumentNullException( nameof( type ) );
            }

            if( _map.Count == 0 )
            {
                value = default;
                return false;
            }

            return _map.TryGetValue( context, out value );
        }

        public bool TrySet( TContext context, Type type, T value )
        {
            return _map.TryAdd( context, value );
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}