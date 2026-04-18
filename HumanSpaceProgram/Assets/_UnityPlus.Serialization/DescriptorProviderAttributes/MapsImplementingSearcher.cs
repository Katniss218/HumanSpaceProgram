using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Searches for a provider registered to an interface implemented by the queried type.
    /// </summary>
    public class MapsImplementingSearcher : IMappingProviderSearcher
    {
        // Key: (Context, InterfaceType)
        private readonly Dictionary<(int, Type), MethodInfo> _map = new Dictionary<(int, Type), MethodInfo>();

        public bool TryGet( int contextId, Type type, out MethodInfo boundMethod )
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            // If the type itself is an interface, check it first
            if( type.IsInterface )
            {
                if( CheckInterface( contextId, type, type, out boundMethod ) )
                    return true;
            }

            // Iterate all implemented interfaces
            foreach( Type interfaceType in type.GetInterfaces() )
            {
                if( CheckInterface( contextId, type, interfaceType, out boundMethod ) )
                    return true;
            }

            boundMethod = default;
            return false;
        }

        private bool CheckInterface( int contextId, Type targetType, Type interfaceType, out MethodInfo boundMethod )
        {
            // 1. Exact Match (e.g. IList<int>)
            if( _map.TryGetValue( (contextId, interfaceType), out MethodInfo rawMethod ) )
            {
                Type[] mappedArgs = ProviderArgsResolver.GetDeconstructedArgs( targetType, interfaceType );
                boundMethod = ProviderBindingUtility.Bind( rawMethod, targetType, mappedArgs );
                if( boundMethod != null ) return true;
            }

            // 2. Generic Definition Match (e.g. IList<>)
            if( interfaceType.IsGenericType && !interfaceType.IsGenericTypeDefinition )
            {
                Type genericDef = interfaceType.GetGenericTypeDefinition();
                if( _map.TryGetValue( (contextId, genericDef), out rawMethod ) )
                {
                    Type[] mappedArgs = ProviderArgsResolver.GetDeconstructedArgs( targetType, genericDef );
                    boundMethod = ProviderBindingUtility.Bind( rawMethod, targetType, mappedArgs );
                    if( boundMethod != null ) return true;
                }
            }

            boundMethod = default;
            return false;
        }

        public bool TrySet( int contextId, Type type, MethodInfo method )
        {
            // type here is the Interface specified in the Attribute
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );
            if( !type.IsInterface )
                throw new ArgumentException( "Type must be an interface", nameof( type ) );

            var key = (contextId, type);
            if( _map.ContainsKey( key ) )
                return false;

            _map[key] = method;
            return true;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}