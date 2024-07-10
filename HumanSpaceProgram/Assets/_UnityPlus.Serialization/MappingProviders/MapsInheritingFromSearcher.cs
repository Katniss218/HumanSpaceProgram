using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A mapping searcher that searches the inheritance chain of objects. This is the basic searcher used to target specific individual types.
    /// </summary>
    public class MapsInheritingFromSearcher<TContext, T> : IMappingProviderSearcher<TContext, T>
    {
        private readonly Dictionary<(TContext, Type), T> _map = new();

        public MapsInheritingFromSearcher()
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

            Type currentTypeToCheck = type;

            while( !_map.TryGetValue( (context, currentTypeToCheck), out value ) )
            {
                if( currentTypeToCheck.IsConstructedGenericType )
                {
                    if( _map.TryGetValue( (context, currentTypeToCheck.GetGenericTypeDefinition()), out value ) )
                    {
                        return true;
                    }
                }

                currentTypeToCheck = currentTypeToCheck.BaseType;
                if( currentTypeToCheck == null )
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Sets the value for the corresponding type.
        /// </summary>
        /// <param name="type">The type to set the value for.</param>
        /// <param name="value">The value to set.</param>
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