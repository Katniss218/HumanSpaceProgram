using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Searches for a provider registered to a base class of the queried type.
    /// </summary>
    public class MapsInheritingFromSearcher : IMappingProviderSearcher
    {
        // Key: (Context, BaseType)
        private readonly Dictionary<(int, Type), MethodInfo> _map = new Dictionary<(int, Type), MethodInfo>();

        public bool TryGet( int contextId, Type type, out MethodInfo boundMethod )
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            Type currentTypeToCheck = type;

            while( currentTypeToCheck != null )
            {
                // 1. Check exact type match
                if( _map.TryGetValue( (contextId, currentTypeToCheck), out MethodInfo rawMethod ) )
                {
                    Type[] mappedArgs = ProviderArgsResolver.GetDeconstructedArgs( type, currentTypeToCheck );
                    boundMethod = ProviderBindingUtility.Bind( rawMethod, type, mappedArgs );
                    if( boundMethod != null ) return true;
                }

                // 2. Check open generic definition (e.g. List<>)
                if( currentTypeToCheck.IsGenericType && !currentTypeToCheck.IsGenericTypeDefinition )
                {
                    Type genericDef = currentTypeToCheck.GetGenericTypeDefinition();
                    if( _map.TryGetValue( (contextId, genericDef), out rawMethod ) )
                    {
                        Type[] mappedArgs = ProviderArgsResolver.GetDeconstructedArgs( type, genericDef );
                        boundMethod = ProviderBindingUtility.Bind( rawMethod, type, mappedArgs );
                        if( boundMethod != null ) return true;
                    }
                }

                currentTypeToCheck = currentTypeToCheck.BaseType;
            }

            boundMethod = default;
            return false;
        }

        public bool TrySet( int contextId, Type type, MethodInfo method )
        {
            // type here is the Base Type specified in the Attribute
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

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