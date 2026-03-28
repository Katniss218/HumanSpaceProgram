using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Searches for a provider registered to a base class of the queried type.
    /// </summary>
    public class MapsInheritingFromSearcher<TContext, T> : IMappingProviderSearcher<TContext, T>
    {
        // Key: (Context, BaseType)
        private readonly Dictionary<(TContext, Type), T> _map = new Dictionary<(TContext, Type), T>();

        public bool TryGet( TContext context, Type type, out T value )
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            Type currentTypeToCheck = type;

            while( currentTypeToCheck != null )
            {
                // 1. Check exact type match
                if( _map.TryGetValue( (context, currentTypeToCheck), out value ) )
                {
                    return true;
                }

                // 2. Check open generic definition (e.g. List<>)
                if( currentTypeToCheck.IsGenericType && !currentTypeToCheck.IsGenericTypeDefinition )
                {
                    if( _map.TryGetValue( (context, currentTypeToCheck.GetGenericTypeDefinition()), out value ) )
                    {
                        return true;
                    }
                }

                currentTypeToCheck = currentTypeToCheck.BaseType;
            }

            value = default;
            return false;
        }

        public bool TrySet( TContext context, Type type, T value )
        {
            // type here is the Base Type specified in the Attribute
            if( type == null ) 
                throw new ArgumentNullException( nameof( type ) );

            var key = (context, type);
            if( _map.ContainsKey( key ) )
                return false;

            _map[key] = value;
            return true;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}