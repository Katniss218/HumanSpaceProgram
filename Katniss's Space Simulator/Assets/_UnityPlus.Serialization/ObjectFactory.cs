using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class ObjectFactory
    {
        private static readonly TypeMap<Func<Type, IForwardReferenceMap, object>> _cache = new();

        public static void ReloadFactoryMethods()
        {
            // Load extension methods `Object ToObject( this SerializedData, IForwardReferenceMap l )`, where 'Object' is the identifier of the type in question, with special chars replaced by `_`.

            // this should also handle immutables.
        }

        /// <summary>
        /// Instantiates an object.
        /// </summary>
        /// <typeparam name="T">The base type that the serialized instance must be compatible with.</typeparam>
        /// <returns>The instantiated object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the saved type ("$type") can't be assigned to type <typeparamref name="T"/>.</exception>
        /// <exception cref="Exception">Other exceptions may be thrown by the factory method. Proceed with caution.</exception>
        public static T AsObject<T>( this SerializedObject data, IForwardReferenceMap l )
        {
            Guid id = data[KeyNames.ID].AsGuid();

            Type type;

            if( data.TryGetValue( KeyNames.TYPE, out var t ) )
            {
                type = t.AsType();

                if( !typeof( T ).IsAssignableFrom( type ) )
                {
                    throw new InvalidOperationException( $"Tried to create an instance of `{type.FullName}` that can't be assigned to `{typeof( T ).FullName}`." );
                }
            }
            else
            {
                type = typeof( T );
            }

            object obj = null;
            if( _cache.TryGetClosest( type, out var factoryFunc ) )
            {
                obj = factoryFunc.Invoke( type, l );
                // Every factory method should add its created instances to the IForwardReferenceMap.
                // Thus, we don't register it here.
            }
            else
            {
                // Default factory.
                obj = Activator.CreateInstance( type );
                l.SetObj( id, obj );
            }

            return (T)obj;
        }
    }
}