using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class SerializationMappingRegistry
    {
        private struct Entry
        {
            public Type targetType;
            public SerializationMapping mapping;
            public MethodInfo method;
            public bool isReady;
        }

        private static readonly TypeMap<int, Entry> _mappings = new();

        private static bool _isInitialized = false;

        private static IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() );
        }

        private static void Initialize()
        {
            foreach( var containingType in GetTypes() )
            {
                MethodInfo[] methods = containingType.GetMethods( BindingFlags.Public | BindingFlags.Static );
                foreach( var method in methods )
                {
                    SerializationMappingProviderAttribute attr = method.GetCustomAttribute<SerializationMappingProviderAttribute>();
                    if( attr == null )
                        continue;

                    if( method.ReturnParameter.ParameterType != typeof( SerializationMapping ) )
                        continue;

                    if( method.GetParameters().Length != 0 )
                        continue;

                    // Find every method that returns a mapping, and cache it.
                    // In case the mapping (and method) is generic, the call is deferred to when the type parameters are known.

                    var entry = new Entry()
                    {
                        targetType = attr.TargetType,
                        mapping = null,
                        method = method,
                        isReady = false
                    };

                    foreach( var context in attr.Contexts )
                    {
                        _mappings.Set( context, attr.TargetType, entry );
                    }
                }
            }

            _isInitialized = true;
        }

        private static Entry MakeReady( int context, Entry entry, Type objType )
        {
            MethodInfo method = entry.method;

            if( method.ContainsGenericParameters )
            {
                // Allows mappings for type 'object' to be genericized with the member type.
                if( entry.targetType == typeof( object ) )
                {
                    method = method.MakeGenericMethod( objType );
                }
                // Arrays need a generic special-case (they technically don't, but it's faster and safer if they have it).
                else if( objType.IsArray )
                {
                    method = method.MakeGenericMethod( objType.GetElementType() );
                }
                // Enums also need a generic special-case to load properly.
                else if( objType.IsEnum )
                {
                    method = method.MakeGenericMethod( objType );
                }
                // Catch-all clause for normal generic types (e.g. List<T>, Dictionary<TKey, TValue), etc).
                // Only call it if special-cases don't match the type.
                else
                {
                    // This may throw an exception if the method is decorated improperly.
                    method = method.MakeGenericMethod( objType.GetGenericArguments() );
                }
            }

            // Get the mapping using the previously found method.
            var mapping = (SerializationMapping)method.Invoke( null, null );
            mapping.context = context;

            // Update everything in the cache.
            entry.method = method;
            entry.isReady = true;
            entry.mapping = mapping;

            _mappings.Set( context, objType, entry );

            return entry;
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given member/variable type.
        /// </summary>
        /// <remarks>
        /// This is useful for mapping manipulation / custom mappings.
        /// </remarks>
        /// <param name="memberType">The type of the member "variable" that the object is/will be assigned to.</param>
        /// <returns>The correct serialization mapping for the given variable type.</returns>
        internal static SerializationMapping GetMappingOrEmpty( int context, Type memberType )
        {
            if( !_isInitialized )
                Initialize();

            if( _mappings.TryGetClosest( context, memberType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    entry = MakeReady( context, entry, memberType );
                    return entry.mapping;
                }

                return entry.mapping;
            }

            return SerializationMapping.Empty( memberType );
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given object.
        /// </summary>
        /// <remarks>
        /// This is useful when serializing.
        /// </remarks>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="memberObj">The object.</param>
        /// <returns>The correct serialization mapping for the given object.</returns>
        public static SerializationMapping GetMappingOrDefault<TMember>( int context, TMember memberObj )
        {
            if( !_isInitialized )
                Initialize();

            Type objType = typeof( TMember );
            if( memberObj != null )
                objType = memberObj.GetType();

            if( _mappings.TryGetClosest( context, objType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    entry = MakeReady( context, entry, objType );
                    return entry.mapping;
                }

                return entry.mapping;
            }

            return SerializationMapping.Empty<TMember>();
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given object.
        /// </summary>
        /// <remarks>
        /// This is useful when deserializing / creating a new object.
        /// </remarks>
        public static SerializationMapping GetMappingOrDefault<TMember>( int context, Type memberType )
        {
            if( !_isInitialized )
                Initialize();

            if( typeof( TMember ).IsAssignableFrom( memberType ) ) // `IsAssignableFrom` doesn't appear to be much of a slow point, surprisingly.
            {
                if( _mappings.TryGetClosest( context, memberType, out var entry ) )
                {
                    if( !entry.isReady )
                    {
                        entry = MakeReady( context, entry, memberType );
                        return entry.mapping;
                    }

                    return entry.mapping;
                }
            }

            return SerializationMapping.Empty<TMember>();
        }
    }
}