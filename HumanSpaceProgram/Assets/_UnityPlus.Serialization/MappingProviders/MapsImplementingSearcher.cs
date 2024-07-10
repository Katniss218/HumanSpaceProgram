using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public class MapsImplementingSearcher<TContext, T> : IMappingProviderSearcher<TContext, T>
    {
        private readonly Dictionary<(TContext, Type), T> _map = new();

        public MapsImplementingSearcher()
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

            // implementing typeof( <interface> ) might be called with either the type of the instance (type that implements the given interface),
            //   or with the type of the interface (if instance is null).
            Type currentTypeToCheck = type;
            if( currentTypeToCheck.IsInterface )
            {
                if( _map.TryGetValue( (context, currentTypeToCheck), out value ) )
                {
                    return true;
                }

                if( currentTypeToCheck.IsConstructedGenericType )
                    currentTypeToCheck = currentTypeToCheck.GetGenericTypeDefinition();

                if( _map.TryGetValue( (context, currentTypeToCheck), out value ) )
                {
                    return true;
                }
            }

            foreach( Type interfaceType in currentTypeToCheck.GetInterfaces() )
            {
                if( _map.TryGetValue( (context, interfaceType), out value ) )
                {
                    return true;
                }
            }

            if( currentTypeToCheck.IsConstructedGenericType )
                currentTypeToCheck = currentTypeToCheck.GetGenericTypeDefinition();

            foreach( Type interfaceType in currentTypeToCheck.GetInterfaces() )
            {
                if( _map.TryGetValue( (context, interfaceType), out value ) )
                {
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TrySet( TContext context, Type type, T value )
        {
            return _map.TryAdd( (context, type), value );
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}