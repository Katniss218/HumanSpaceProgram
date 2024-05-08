using System;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class ObjectFactory
    {
        public struct Info
        {
            public Delegate factory;
            public bool isConstructedGeneric;
            public bool isPrimitiveLike;
        }

        private static readonly TypeMap<Info> _cache = new();

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
        internal static T CreateObject<T>( SerializedData data, IForwardReferenceMap l )
        {
            Type type;

#warning TODO - this probably shouldn't be here.
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

            if( _cache.TryGetClosest( type, out var factoryFunc ) )
            {
                if( type.IsConstructedGenericType && !factoryFunc.isConstructedGeneric )
                {
                    Type[] genericArguments = type.GetGenericArguments();

                    // cache will initially return you the unconstructed method (because constructed ones don't exist).
                    MethodInfo method = factoryFunc.factory.Method;
                    MethodInfo genericMethod = method.MakeGenericMethod( genericArguments );

                    factoryFunc.factory = Delegate.CreateDelegate( ..., genericMethod );
                    factoryFunc.isConstructedGeneric = true;

                    _cache.Set( type, factoryFunc );
                }

                // Every factory method should add its created instances to the IForwardReferenceMap.
                // Thus, we don't register it here.
                return (T)factoryFunc.factory.DynamicInvoke( data, l );
            }

            T obj = (T)Activator.CreateInstance( type );

            if( data.TryGetValue( KeyNames.ID, out var id ) )
            {
                l.SetObj( id.AsGuid(), obj );
            }

            return obj;
        }
    }
}