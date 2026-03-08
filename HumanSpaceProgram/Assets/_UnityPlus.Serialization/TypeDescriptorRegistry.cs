using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class TypeDescriptorRegistry
    {
        // Cache: (Type, Context) -> Descriptor
        private static readonly Dictionary<(Type, int), IDescriptor> _descriptors = new Dictionary<(Type, int), IDescriptor>();

        // Provider Lookups (V3 Style - Generalized)
        private static readonly MapsInheritingFromSearcher<int, MethodInfo> _inheritingSearcher = new();
        private static readonly MapsImplementingSearcher<int, MethodInfo> _implementingSearcher = new();
        private static readonly MapsAnyClassSearcher<int, MethodInfo> _anyClassSearcher = new();
        private static readonly MapsAnyStructSearcher<int, MethodInfo> _anyStructSearcher = new();
        private static readonly MapsAnyInterfaceSearcher<int, MethodInfo> _anyInterfaceSearcher = new();
        private static readonly MapsAnySearcher<int, MethodInfo> _anySearcher = new();

        // Extensions: (TargetType, Context) -> List of Extension Methods
        private static readonly Dictionary<(Type, int), List<MethodInfo>> _extensions = new Dictionary<(Type, int), List<MethodInfo>>();

        private static bool _isInitialized = false;

        private static void Initialize()
        {
            if( _isInitialized )
                return;

            // Force initialization of compatibility context constants before reflecting on them
#pragma warning disable CS0618 // Type or member is obsolete
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( ObjectContext ).TypeHandle );
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( ArrayContext ).TypeHandle );
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( KeyValueContext ).TypeHandle );
#pragma warning restore CS0618

            // Scan all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies )
            {
                string name = assembly.GetName().Name;
                if( name.StartsWith( "System" ) || name.StartsWith( "mscorlib" ) || name.StartsWith( "UnityEditor" ) )
                    continue;

                foreach( var type in assembly.GetTypes() )
                {
                    // Scan methods
                    var methods = type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
                    foreach( var method in methods )
                    {
                        // Extensions
                        var extAttributes = method.GetCustomAttributes<ExtendsMappingOfAttribute>( false );
                        foreach( var attr in extAttributes )
                        {
                            var key = (attr.TargetType, attr.Context);
                            if( !_extensions.ContainsKey( key ) )
                                _extensions[key] = new List<MethodInfo>();

                            _extensions[key].Add( method );
                        }

                        // Mapping Providers
                        var providerAttributes = method.GetCustomAttributes<MappingProviderAttribute>( false );
                        foreach( var attr in providerAttributes )
                        {
                            if( !typeof( IDescriptor ).IsAssignableFrom( method.ReturnType ) ) continue;

                            IEnumerable<int> targetContexts;
                            if( attr.ContextType != null )
                            {
                                int id = ContextRegistry.GetID( attr.ContextType ).ID;
                                targetContexts = new int[] { id };
                            }
                            else
                            {
                                targetContexts = attr.Contexts;
                            }

                            foreach( var ctx in targetContexts )
                            {
                                if( attr is MapsInheritingFromAttribute inh )
                                    _inheritingSearcher.TrySet( ctx, inh.MappedType, method );

                                else if( attr is MapsImplementingAttribute imp )
                                    _implementingSearcher.TrySet( ctx, imp.MappedType, method );

                                else if( attr is MapsAnyClassAttribute )
                                    _anyClassSearcher.TrySet( ctx, null, method );

                                else if( attr is MapsAnyStructAttribute )
                                    _anyStructSearcher.TrySet( ctx, null, method );

                                else if( attr is MapsAnyInterfaceAttribute )
                                    _anyInterfaceSearcher.TrySet( ctx, null, method );

                                else if( attr is MapsAnyAttribute )
                                    _anySearcher.TrySet( ctx, null, method );
                            }
                        }
                    }

                }
            }

            _isInitialized = true;
        }

        public static void Register( IDescriptor descriptor, ContextKey context = default )
        {
            if( descriptor == null )
                return;
            _descriptors[(descriptor.MappedType, context.ID)] = descriptor;
        }

        public static IDescriptor GetDescriptor( Type type, ContextKey context = default )
        {
            if( type == null )
                return null;

            if( !_isInitialized )
                Initialize();

            if( _descriptors.TryGetValue( (type, context.ID), out var descriptor ) )
            {
                return descriptor;
            }

            descriptor = CreateDescriptor( type, context );
            if( descriptor != null )
            {
                ApplyExtensions( descriptor, type, context );
                Register( descriptor, context );
            }

            return descriptor;
        }

        public static void Clear()
        {
            _descriptors.Clear();

            _inheritingSearcher.Clear();
            _implementingSearcher.Clear();
            _anyClassSearcher.Clear();
            _anyStructSearcher.Clear();
            _anyInterfaceSearcher.Clear();
            _anySearcher.Clear();

            _extensions.Clear();

            _isInitialized = false;
        }

        private static void ApplyExtensions( IDescriptor descriptor, Type type, ContextKey context )
        {
            if( _extensions.TryGetValue( (type, context.ID), out var methods ) )
            {
                object[] args = new object[] { descriptor };
                foreach( var method in methods )
                {
                    try
                    {
                        method.Invoke( null, args );
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Error applying extension '{method.Name}' to descriptor for {type.Name}: {ex}" );
                    }
                }
            }
        }

        /// <summary>
        /// Creates a descriptor instance for the specified type and context using resolution order.
        /// </summary>
        private static IDescriptor CreateDescriptor( Type type, ContextKey context )
        {
            // Traverse context hierarchy (Specific ID -> Generic Def ID -> Default ID)
            // This allows a provider registered for Ctx.Dict<,> to handle Ctx.Dict<Int,Int> requests.
            IReadOnlyList<int> contextHierarchy = ContextRegistry.GetContextHierarchy( context.ID );

            foreach( int contextId in contextHierarchy )
            {
                MethodInfo method;

                // --- 2. Inheritance Hierarchy (Specific Bases) ---
                if( _inheritingSearcher.TryGet( contextId, type, out method ) )
                {
                    var desc = InvokeProvider( method, type, context );
                    if( desc != null ) return desc;
                }

                // --- 3. Interfaces ---
                if( _implementingSearcher.TryGet( contextId, type, out method ) )
                {
                    var desc = InvokeProvider( method, type, context );
                    if( desc != null ) return desc;
                }

                // --- 5. Category Fallbacks (Any Class/Struct/Interface) ---
                if( _anyClassSearcher.TryGet( contextId, type, out method ) )
                {
                    var desc = InvokeProvider( method, type, context );
                    if( desc != null ) return desc;
                }

                if( _anyStructSearcher.TryGet( contextId, type, out method ) )
                {
                    var desc = InvokeProvider( method, type, context );
                    if( desc != null ) return desc;
                }

                if( _anyInterfaceSearcher.TryGet( contextId, type, out method ) )
                {
                    var desc = InvokeProvider( method, type, context );
                    if( desc != null ) return desc;
                }

                // --- 6. Absolute Fallback (Any) ---
                if( _anySearcher.TryGet( contextId, type, out method ) )
                {
                    var desc = InvokeProvider( method, type, context );
                    if( desc != null ) return desc;
                }

                if( context == ContextKey.Default )
                {
                    break;
                }
            }

            // --- Reflection Fallback (auto-generated provider) ---
            if( type.IsClass || type.IsValueType || type.IsInterface )
            {
                Type descType = typeof( ReflectionClassDescriptor<> ).MakeGenericType( type );
                return (IDescriptor)Activator.CreateInstance( descType );
            }

            return null;
        }

        private static IDescriptor InvokeProvider( MethodInfo method, Type targetType, ContextKey context )
        {
            try
            {
                // Determine Generic Arguments based on the Target Type
                Type[] genericArgs;
                if( targetType.IsArray )
                {
                    genericArgs = new Type[] { targetType.GetElementType() };
                }
                else if( targetType.IsGenericType )
                {
                    genericArgs = targetType.GetGenericArguments();
                }
                else if( targetType.IsEnum )
                {
                    genericArgs = new Type[] { targetType };
                }
                else
                {
                    /*
                    if( method.GetGenericArguments().Length != objType.GetGenericArguments().Length )
                    {
                        throw new InvalidOperationException( $"Couldn't initialize mapping from method `{method}` (mapped type: `{objType}`). Number of generic parameters on the method doesn't match the number of generic parameters on the mapped type." );
                    }*/
                    genericArgs = Type.EmptyTypes;
                }

                // CASE 1: The Provider is inside a Generic Class (e.g. class Provider<T> { static Method() } )
                if( method.DeclaringType.IsGenericTypeDefinition )
                {
                    // We must close the declaring type with the generic args
                    Type closedProviderType = method.DeclaringType.MakeGenericType( genericArgs );

                    // We must find the matching method on the closed type. 
                    // MethodBase.GetMethodFromHandle is the most robust way to map Open Method -> Closed Method
                    method = (MethodInfo)MethodBase.GetMethodFromHandle( method.MethodHandle, closedProviderType.TypeHandle );
                }
                // CASE 2: The Provider Method itself is Generic (e.g. static Method<T>() )
                else if( method.IsGenericMethodDefinition )
                {
                    try
                    {
                        // Safety check: ensure generic args match method definition count
                        if( method.GetGenericArguments().Length == genericArgs.Length )
                        {
                            method = method.MakeGenericMethod( genericArgs );
                        }
                        else if( method.GetGenericArguments().Length == 1 && genericArgs.Length == 0 )
                        {
                            // Special case: Method<T> called for non-generic type (e.g. MapsAnyClass -> T is the type itself)
                            method = method.MakeGenericMethod( targetType );
                        }
                    }
                    catch( ArgumentException )
                    {
                        // The target type does not satisfy the generic constraints of the provider method.
                        // This means the provider is not applicable to this specific type.
                        return null;
                    }
                }

                // Inject Context if requested
                ParameterInfo[] paramsInfo = method.GetParameters();
                object[] args = null;

                if( paramsInfo.Length == 1 && paramsInfo[0].ParameterType == typeof( ContextKey ) )
                {
                    args = new object[] { context };
                }

                return (IDescriptor)method.Invoke( null, args );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to invoke provider '{method.Name}' for type '{targetType}': {ex}" );
                return null;
            }
        }
    }
}