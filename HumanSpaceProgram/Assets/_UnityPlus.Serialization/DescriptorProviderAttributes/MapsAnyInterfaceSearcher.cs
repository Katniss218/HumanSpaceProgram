using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityPlus.Serialization
{
    public class MapsAnyInterfaceSearcher : IMappingProviderSearcher
    {
        private readonly Dictionary<int, MethodInfo> _map = new Dictionary<int, MethodInfo>();

        public bool TryGet( int contextId, Type type, out MethodInfo boundMethod )
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            if( type.IsInterface )
            {
                if( _map.TryGetValue( contextId, out MethodInfo rawMethod ) )
                {
                    Type[] mappedArgs = ProviderArgsResolver.GetDeconstructedArgs( type, null );
                    boundMethod = ProviderBindingUtility.Bind( rawMethod, type, mappedArgs );
                    if( boundMethod != null ) return true;
                }
            }

            boundMethod = default;
            return false;
        }

        public bool TrySet( int contextId, Type type, MethodInfo method )
        {
            if( _map.ContainsKey( contextId ) )
                return false;
            _map[contextId] = method;
            return true;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}