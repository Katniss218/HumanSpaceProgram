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
            public SerializationMapping mapping;
            public MethodInfo method;
            public bool isReady;
        }

        private static readonly TypeMap<Entry> _mappings = new();

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
                        mapping = null,
                        method = method,
                        isReady = false
                    };

                    _mappings.Set( attr.TargetType, entry );
                }
            }

            _isInitialized = true;
        }

        private static Entry MakeReady( Entry entry, Type objType )
        {
            // Get the mapping using the previously found method.
            // If the method is generic, we can fill in the generic parameters using the generic parameters of the type we want to save/load.
            MethodInfo method = entry.method;

            if( method.ContainsGenericParameters )
            {
                // Arrays need a generic special-case (they technically don't, but it's faster and safer if they have it).
                if( objType.IsArray )
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
                    method = method.MakeGenericMethod( objType.GetGenericArguments() );
                }
            }

            var mapping = (SerializationMapping)method.Invoke( null, null );
            entry.method = method; // Update with generic definition.
            entry.isReady = true;
            entry.mapping = mapping;

            _mappings.Set( objType, entry );
            return entry;
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given member type.
        /// </summary>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="memberObj">The object.</param>
        /// <returns>The correct serialization mapping for the given object.</returns>
        internal static SerializationMapping GetMappingOrEmpty( Type memberType )
        {
            if( !_isInitialized )
                Initialize();

            if( _mappings.TryGetClosest( memberType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    entry = MakeReady( entry, memberType );
                    return entry.mapping;
                }

                return entry.mapping;
            }

            return SerializationMapping.Empty( memberType );
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given member type.
        /// </summary>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="memberObj">The object.</param>
        /// <returns>The correct serialization mapping for the given object.</returns>
        public static SerializationMapping GetMappingOrDefault<TMember>( TMember memberObj )
        {
            if( !_isInitialized )
                Initialize();

            Type objType = typeof( TMember );
            if( memberObj != null )
                objType = memberObj.GetType();

            if( _mappings.TryGetClosest( objType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    entry = MakeReady( entry, objType );
                    return entry.mapping;
                }

                return entry.mapping;
            }

            return SerializationMapping.Empty<TMember>();
        }

        public static SerializationMapping GetMappingOrDefault<TMember>( Type memberType )
        {
            if( !_isInitialized )
                Initialize();

            if( typeof( TMember ).IsAssignableFrom( memberType ) ) // `IsAssignableFrom` doesn't appear to be much of a slow point, surprisingly.
            {
                if( _mappings.TryGetClosest( memberType, out var entry ) )
                {
                    if( !entry.isReady )
                    {
                        entry = MakeReady( entry, memberType );
                        return entry.mapping;
                    }

                    return entry.mapping;
                }
            }

            return SerializationMapping.Empty<TMember>();
        }
    }
}