using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Searches for a provider registered to an interface implemented by the queried type.
    /// </summary>
    public class MapsImplementingSearcher<TContext, T> : IMappingProviderSearcher<TContext, T>
    {
        // Key: (Context, InterfaceType)
        private readonly Dictionary<(TContext, Type), T> _map = new Dictionary<(TContext, Type), T>();

        public bool TryGet( TContext context, Type type, out T value )
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            // If the type itself is an interface, check it first
            if( type.IsInterface )
            {
                if( CheckInterface( context, type, out value ) )
                    return true;
            }

            // Iterate all implemented interfaces
            foreach( Type interfaceType in type.GetInterfaces() )
            {
                if( CheckInterface( context, interfaceType, out value ) )
                    return true;
            }

            value = default;
            return false;
        }

        private bool CheckInterface( TContext context, Type interfaceType, out T value )
        {
            // 1. Exact Match (e.g. IList<int>)
            if( _map.TryGetValue( (context, interfaceType), out value ) )
                return true;

            // 2. Generic Definition Match (e.g. IList<>)
            if( interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition )
            {
                if( _map.TryGetValue( (context, interfaceType.GetGenericTypeDefinition()), out value ) )
                    return true;
            }

            value = default;
            return false;
        }

        public bool TrySet( TContext context, Type type, T value )
        {
            // type here is the Interface specified in the Attribute
            if( type == null ) 
                throw new ArgumentNullException( nameof( type ) );
            if( !type.IsInterface ) 
                throw new ArgumentException( "Type must be an interface", nameof( type ) );

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