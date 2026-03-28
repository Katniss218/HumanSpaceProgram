using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class MapsAnyStructSearcher<TContext, T> : IMappingProviderSearcher<TContext, T>
    {
        private readonly Dictionary<TContext, T> _map = new Dictionary<TContext, T>();

        public bool TryGet( TContext context, Type type, out T value )
        {
            if( type == null ) 
                throw new ArgumentNullException( nameof( type ) );

            if( type.IsValueType && !type.IsEnum && !type.IsPrimitive )
            {
                return _map.TryGetValue( context, out value );
            }

            value = default;
            return false;
        }

        public bool TrySet( TContext context, Type type, T value )
        {
            if( _map.ContainsKey( context ) )
                return false;
            _map[context] = value;
            return true;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}