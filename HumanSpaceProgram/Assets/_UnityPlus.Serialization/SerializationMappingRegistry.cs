using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Manages the mappings defined in the program.
    /// </summary>
    public static class SerializationMappingRegistry
    {
        private struct MappingGetterMethod
        {
            public Type mappedType;
            public MethodInfo method;
            public MappingProviderAttribute providerAttribute;
        }

        private static readonly Dictionary<(int, Type), SerializationMapping> _directCache = new();

        private static readonly MapsInheritingFromSearcher<int, MappingGetterMethod> _inheritingFromMappings = new();
        private static readonly MapsImplementingSearcher<int, MappingGetterMethod> _implementingMappings = new();
        private static readonly MapsAnyClassSearcher<int, MappingGetterMethod> _anyClassMappings = new();
        private static readonly MapsAnyStructSearcher<int, MappingGetterMethod> _anyStructMappings = new();
        private static readonly MapsAnyInterfaceSearcher<int, MappingGetterMethod> _anyInterfaceMappings = new();
        private static readonly MapsAnySearcher<int, MappingGetterMethod> _anyMappings = new();

        private static bool _isInitialized = false;

        private static IEnumerable<Type> GetTypes( IEnumerable<Assembly> assemblies )
        {
            return assemblies
                .SelectMany( a => a.GetTypes() )
                .Where( t => !t.IsGenericType ); // Menetic types don't work in this context, so we skip them when searching for methods returning mappings.
                                                 // The method itself can still be generic.
        }

        private static void Initialize( IEnumerable<Assembly> assemblies )
        {
            foreach( var containingType in GetTypes( assemblies ) )
            {
                MethodInfo[] methods = containingType.GetMethods( BindingFlags.Public | BindingFlags.Static );
                foreach( var method in methods )
                {
                    // Find every method that returns a mapping, and cache *that method*.
                    // The invocation is deferred to when the type parameters are known (that is, when the mapping is retrieved).
                    //   This allows for having generic mapping getter methods.

                    if( method.ReturnParameter.ParameterType != typeof( SerializationMapping ) )
                        continue;

                    if( method.GetParameters().Length != 0 )
                        continue;

                    IEnumerable<MappingProviderAttribute> attrs = method.GetCustomAttributes<MappingProviderAttribute>();
                    if( attrs == null )
                        continue;

                    foreach( var attr in attrs )
                    {
                        var entry = new MappingGetterMethod()
                        {
                            mappedType = attr.MappedType,
                            method = method,
                            providerAttribute = attr,
                        };

                        foreach( var context in attr.Contexts )
                        {
                            switch( attr )
                            {
                                case MapsInheritingFromAttribute:

                                    if( !_inheritingFromMappings.TrySet( context, attr.MappedType, entry ) )
                                    {
                                        Debug.LogWarning( $"Multiple '{nameof( MapsInheritingFromAttribute )}' mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );
                                    }
                                    break;
                                case MapsImplementingAttribute:

                                    if( !_implementingMappings.TrySet( context, attr.MappedType, entry ) )
                                    {
                                        Debug.LogWarning( $"Multiple '{nameof( MapsImplementingAttribute )}' mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );
                                    }
                                    break;
                                case MapsAnyClassAttribute:

                                    if( !_anyClassMappings.TrySet( context, attr.MappedType, entry ) )
                                    {
                                        Debug.LogWarning( $"Multiple '{nameof( MapsAnyClassAttribute )}' mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );
                                    }
                                    break;
                                case MapsAnyStructAttribute:

                                    if( !_anyStructMappings.TrySet( context, attr.MappedType, entry ) )
                                    {
                                        Debug.LogWarning( $"Multiple '{nameof( MapsAnyStructAttribute )}' mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );
                                    }
                                    break;
                                case MapsAnyInterfaceAttribute:

                                    if( !_anyInterfaceMappings.TrySet( context, attr.MappedType, entry ) )
                                    {
                                        Debug.LogWarning( $"Multiple '{nameof( MapsAnyInterfaceAttribute )}' mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );
                                    }
                                    break;
                                case MapsAnyAttribute:

                                    if( !_anyMappings.TrySet( context, attr.MappedType, entry ) )
                                    {
                                        Debug.LogWarning( $"Multiple '{nameof( MapsAnyAttribute )}' mappings found for type `{attr.MappedType.AssemblyQualifiedName}`." );
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Forces the registry to reload all its mappings.
        /// </summary>
        /// <remarks>
        /// Mappings for unloaded types will not be removed from the registry.
        /// </remarks>
        public static void ForceReload()
        {
            Initialize( AppDomain.CurrentDomain.GetAssemblies() );
        }

        /// <summary>
        /// Forces the registry to discard and reload all its mappings.
        /// </summary>
        /// <remarks>
        /// Mappings for unloaded types will not be removed from the registry.
        /// </remarks>
        public static void ForceReload( Assembly assembly )
        {
            Initialize( new Assembly[] { assembly } );
        }

        private static SerializationMapping MakeReadyAndRegister( int context, MappingGetterMethod entry, Type objType )
        {
            MethodInfo method = entry.method;

            if( method.ContainsGenericParameters )
            {
                if( entry.providerAttribute is MapsAnyClassAttribute )
                {
                    method = method.MakeGenericMethod( objType );
                }
                else if( entry.providerAttribute is MapsAnyStructAttribute )
                {
                    method = method.MakeGenericMethod( objType );
                }
                else if( entry.providerAttribute is MapsAnyInterfaceAttribute )
                {
                    method = method.MakeGenericMethod( objType );
                }
                else if( entry.providerAttribute is MapsAnyAttribute )
                {
                    method = method.MakeGenericMethod( objType );
                }

                // Allows mappings for type 'object' to be genericized with the member type.
                else if( entry.mappedType == typeof( object ) )
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
            mapping.Context = context;

            _directCache[(context, objType)] = mapping;

            return mapping;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static SerializationMapping GetMappingInternal( int context, Type type )
        {
            if( _directCache.TryGetValue( (context, type), out var mapping ) )
            {
                if( mapping == null )
                    return null;

                return mapping.GetInstance();
            }

            if( _inheritingFromMappings.TryGet( context, type, out var entry )
             || _implementingMappings.TryGet( context, type, out entry )
             || _anyInterfaceMappings.TryGet( context, type, out entry )
             || _anyClassMappings.TryGet( context, type, out entry )
             || _anyStructMappings.TryGet( context, type, out entry )
             || _anyMappings.TryGet( context, type, out entry ) )
            {
                mapping = MakeReadyAndRegister( context, entry, type );

                if( mapping == null )
                    return null;

                return mapping.GetInstance();
            }

            _directCache[(context, type)] = null;

            return null;
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
                Initialize( AppDomain.CurrentDomain.GetAssemblies() );

            return GetMappingInternal( context, memberType );
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
                Initialize( AppDomain.CurrentDomain.GetAssemblies() );

            Type objType = memberObj == null
                ? typeof( TMember )
                : memberObj.GetType();

            return GetMappingInternal( context, objType );
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
                Initialize( AppDomain.CurrentDomain.GetAssemblies() );

            if( objType == null )
                objType = typeof( TMember );

            if( typeof( TMember ).IsAssignableFrom( objType ) ) // `IsAssignableFrom` appears to be quite fast, surprisingly.
            {
                return GetMappingInternal( context, objType );
            }

            return null;
        }
    }
}