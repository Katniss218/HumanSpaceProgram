using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class SerializationMappingRegistry
    {
        private struct RegistryEntry
        {
            public bool isReady;

            public Type mappedType;
            public MethodInfo method;
            public SerializationMapping mapping;
        }

        private static readonly TypeMap<int, RegistryEntry> _mappings = new();

        private static bool _isInitialized = false;

        private static IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( t => !t.IsGenericType ); // Menetic types don't work in this context, so we skip them when searching for methods returning mappings.
                                                 // The method itself can still be generic.
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

                    var entry = new RegistryEntry()
                    {
                        mappedType = attr.MappedType,
                        mapping = null,
                        method = method,
                        isReady = false
                    };

                    foreach( var context in attr.Contexts )
                    {
                        if( _mappings.TryGet( context, attr.MappedType, out _ ) )
                            Debug.LogWarning( $"Multiple mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );

                        _mappings.Set( context, attr.MappedType, entry );
                    }
                }
            }

            _isInitialized = true;
        }

        private static RegistryEntry MakeReadyAndRegister( int context, RegistryEntry entry, Type objType )
        {
            MethodInfo method = entry.method;

            if( method.ContainsGenericParameters )
            {
                // Allows mappings for type 'object' to be genericized with the member type.
                if( entry.mappedType == typeof( object ) )
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
                    if( method.GetGenericArguments().Length != objType.GetGenericArguments().Length )
                    {
                        throw new InvalidOperationException( $"Couldn't initialize mapping from method `{method}` (mapped type: `{objType}`). Number of generic parameters on the method doesn't match the number of generic parameters on the object type." );
                    }

                    // This may still throw an exception if the method has additional generic constraints.
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
        /// <returns>The correct serialization mapping for the given member type.</returns>
        public static SerializationMapping GetMappingOrNull( int context, Type memberType )
        {
            if( memberType == null )
            {
                throw new ArgumentNullException( nameof( memberType ), $"The type of the member can't be null" );
            }

            if( !_isInitialized )
                Initialize();

            if( _mappings.TryGetClosest( context, memberType, out var entry ) )
            {
                if( !entry.isReady )
                {
                    entry = MakeReadyAndRegister( context, entry, memberType );
                }

                if( entry.mapping == null )
                    return null;

                return entry.mapping.GetWorkingInstance();
            }

            entry = new RegistryEntry()
            {
                isReady = true,
                mappedType = memberType,
                method = null,
                mapping = null
            };

            _mappings.Set( context, memberType, entry );

            return null;
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given object.
        /// </summary>
        /// <remarks>
        /// This is useful when saving.
        /// </remarks>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="memberObj">The object.</param>
        /// <returns>The correct serialization mapping for the given member+object pair.</returns>
        public static SerializationMapping GetMapping<TMember>( int context, TMember memberObj )
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
                    entry = MakeReadyAndRegister( context, entry, objType );
                }

                if( entry.mapping == null )
                    return null;

                return entry.mapping.GetWorkingInstance();
            }

            entry = new RegistryEntry()
            {
                isReady = true,
                mappedType = objType,
                method = null,
                mapping = null
            };

            _mappings.Set( context, objType, entry );

            return null;
        }

        /// <summary>
        /// Retrieves a serialization mapping for the given object.
        /// </summary>
        /// <remarks>
        /// This is useful when loading and creating new objects.
        /// </remarks>
        /// <typeparam name="TMember">The type of the member ("variable") that the object is/will be assigned to.</typeparam>
        /// <param name="objType">The actual type of the object in question.</param>
        /// <returns>The correct serialization mapping for the given member+object pair.</returns>
        public static SerializationMapping GetMapping<TMember>( int context, Type objType )
        {
            if( !_isInitialized )
                Initialize();

            if( objType == null )
                objType = typeof( TMember );

            if( typeof( TMember ).IsAssignableFrom( objType ) ) // `IsAssignableFrom` appears to be quite fast, surprisingly.
            {
                if( _mappings.TryGetClosest( context, objType, out var entry ) )
                {
                    if( !entry.isReady )
                    {
                        entry = MakeReadyAndRegister( context, entry, objType );
                    }

                    if( entry.mapping == null )
                        return null;

                    return entry.mapping.GetWorkingInstance();
                }

                entry = new RegistryEntry()
                {
                    isReady = true,
                    mappedType = objType,
                    method = null,
                    mapping = null
                };

                _mappings.Set( context, objType, entry );
            }

            return null;
        }
    }
}