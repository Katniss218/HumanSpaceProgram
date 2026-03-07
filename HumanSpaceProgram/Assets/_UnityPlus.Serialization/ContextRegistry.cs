
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps context marker types to unique IDs for the duration of the execution of the application.
    /// Also manages the structural rules of contexts (e.g. A List in Context X contains Elements in Context Y).
    /// </summary>
    public static class ContextRegistry
    {
        private static readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private static readonly Dictionary<int, Type> _idToType = new Dictionary<int, Type>(); // Reverse lookup for debugging
        private static readonly Dictionary<int, string> _idToName = new Dictionary<int, string>(); // Name lookup

        // Start dynamic IDs high to avoid collision with legacy v3 constants
        private static int _nextDynamicId = 2000000;

        // Generic cache: "GenericTypeDefName + ArgIDs" -> ContextID
        private static readonly Dictionary<string, int> _genericCombinations = new Dictionary<string, int>();

        // Selectors: Map a Context ID to its selection logic.
        private static readonly Dictionary<int, IContextSelector> _selectors = new Dictionary<int, IContextSelector>();

        // Cache for context hierarchies
        private static readonly Dictionary<int, int[]> _hierarchyCache = new Dictionary<int, int[]>();

        static ContextRegistry()
        {
            // Register Core Contexts
            Register( typeof( Ctx.Value ), ContextIDs.Default );
            Register( typeof( Ctx.Asset ), ContextIDs.Asset );
            Register( typeof( Ctx.Ref ), ContextIDs.Ref );

            RegisterName( ContextIDs.Default, "Default" );
            RegisterName( ContextIDs.Ref, "Reference" );
            RegisterName( ContextIDs.Asset, "Asset" );

            // Register Array Contexts
            Register( typeof( Ctx.Array<Ctx.Ref> ), ContextIDs.ArrayRefs );
            Register( typeof( Ctx.Array<Ctx.Asset> ), ContextIDs.ArrayAssets );

            RegisterContextArguments( new ContextKey( ContextIDs.ArrayRefs ), new ContextKey( ContextIDs.Ref ) );
            RegisterContextArguments( new ContextKey( ContextIDs.ArrayAssets ), new ContextKey( ContextIDs.Asset ) );

            RegisterName( ContextIDs.ArrayRefs, "Array<Ref>" );
            RegisterName( ContextIDs.ArrayAssets, "Array<Asset>" );

            // Register Dictionary Contexts
            Register( typeof( Ctx.KeyValue<Ctx.Value, Ctx.Ref> ), ContextIDs.DictValueToRef );
            Register( typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Value> ), ContextIDs.DictRefToValue );
            Register( typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Ref> ), ContextIDs.DictRefToRef );
            Register( typeof( Ctx.KeyValue<Ctx.Value, Ctx.Asset> ), ContextIDs.DictValueToAsset );
            Register( typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Asset> ), ContextIDs.DictRefToAsset );

            RegisterContextArguments( new ContextKey( ContextIDs.DictValueToRef ), new ContextKey( ContextIDs.Default ), new ContextKey( ContextIDs.Ref ) );
            RegisterContextArguments( new ContextKey( ContextIDs.DictRefToValue ), new ContextKey( ContextIDs.Ref ), new ContextKey( ContextIDs.Default ) );
            RegisterContextArguments( new ContextKey( ContextIDs.DictRefToRef ), new ContextKey( ContextIDs.Ref ), new ContextKey( ContextIDs.Ref ) );
            RegisterContextArguments( new ContextKey( ContextIDs.DictValueToAsset ), new ContextKey( ContextIDs.Default ), new ContextKey( ContextIDs.Asset ) );
            RegisterContextArguments( new ContextKey( ContextIDs.DictRefToAsset ), new ContextKey( ContextIDs.Ref ), new ContextKey( ContextIDs.Asset ) );

            RegisterName( ContextIDs.DictValueToRef, "Dict<Default, Ref>" );
            RegisterName( ContextIDs.DictRefToValue, "Dict<Ref, Default>" );
            RegisterName( ContextIDs.DictRefToRef, "Dict<Ref, Ref>" );
            RegisterName( ContextIDs.DictValueToAsset, "Dict<Default, Asset>" );
            RegisterName( ContextIDs.DictRefToAsset, "Dict<Ref, Asset>" );
        }

        /// <summary>
        /// Registers a fixed mapping between a Type and an ID. 
        /// Used for v3 backward compatibility.
        /// </summary>
        [Obsolete( "Use GetID to automatically register context types and GetOrRegisterGenericContext for generic contexts instead of fixed mappings" )]
        public static void Register( Type type, int id )
        {
            if( type == null ) return;
            _typeToId[type] = id;
            _idToType[id] = type;
        }

        public static void RegisterName( int id, string name )
        {
            _idToName[id] = name;
        }

        public static string GetContextName( ContextKey key )
        {
            if( _idToName.TryGetValue( key.ID, out string name ) ) return name;
            if( _idToType.TryGetValue( key.ID, out Type t ) ) return t.Name;
            return key.ID == 0 ? "Default" : $"Context_{key.ID}";
        }

        public static Type GetContextType( ContextKey key )
        {
            return _idToType.TryGetValue( key.ID, out Type t ) ? t : null;
        }

        public static IReadOnlyList<int> GetContextHierarchy( int contextId )
        {
            if( contextId == ContextIDs.Default )
                return new int[] { ContextIDs.Default };

            if( _hierarchyCache.TryGetValue( contextId, out int[] cached ) )
                return cached;

            List<int> hierarchy = new List<int>()
            {
                contextId
            };

            if( _idToType.TryGetValue( contextId, out Type type ) )
            {
                if( type.IsGenericType && !type.IsGenericTypeDefinition )
                {
                    Type genericDef = type.GetGenericTypeDefinition();
                    ContextKey defKey = GetID( genericDef );
                    if( defKey.ID != contextId && defKey.ID != ContextIDs.Default )
                    {
                        hierarchy.Add( defKey.ID );
                    }
                }
            }

            if( contextId != ContextIDs.Default )
            {
                hierarchy.Add( ContextIDs.Default );
            }

            int[] result = hierarchy.ToArray();
            _hierarchyCache[contextId] = result;
            return result;
        }

        /// <summary>
        /// Registers a selector for the given context.
        /// </summary>
        public static void RegisterSelector( ContextKey context, IContextSelector selector )
        {
            _selectors[context.ID] = selector;
        }

        /// <summary>
        /// Legacy compatibility: Registers a UniformSelector for the given arguments.
        /// </summary>
        [Obsolete( "Use RegisterSelector with UniformSelector or a custom IContextSelector for more complex selection logic" )]
        public static void RegisterContextArguments( ContextKey context, params ContextKey[] args )
        {
            _selectors[context.ID] = new UniformSelector( args );
        }

        public static IContextSelector GetSelector( ContextKey context )
        {
            return _selectors.TryGetValue( context.ID, out var selector )
                ? selector
                : new UniformSelector( ContextKey.Default );
        }

        /// <summary>
        /// Resolves the context for a specific child element based on the registered Selector.
        /// </summary>
        /// <param name="parentContext">The context of the array, when invoked on array element, and so on.</param>
        public static ContextKey Resolve( ContextKey parentContext, string key, Type declaredType, Type actualType, int containerCount = -1 )
        {
            if( _selectors.TryGetValue( parentContext.ID, out var selector ) )
            {
                var args = new ContextSelectionArgs( key, declaredType, actualType, containerCount );
                return selector.Select( args );
            }
            return ContextIDs.Default;
        }

        /// <summary>
        /// Resolves the context for a specific child element based on the registered Selector.
        /// </summary>
        /// <param name="parentContext">The context of the array, when invoked on array element, and so on.</param>
        public static ContextKey Resolve( ContextKey parentContext, int index, Type declaredType, Type actualType, int containerCount = -1 )
        {
            if( _selectors.TryGetValue( parentContext.ID, out var selector ) )
            {
                var args = new ContextSelectionArgs( index, declaredType, actualType, containerCount );
                return selector.Select( args );
            }
            return ContextIDs.Default;
        }

        public static bool IsGenericContext( ContextKey context )
        {
            return _selectors.TryGetValue( context.ID, out var selector ) && selector is UniformSelector;
        }

        public static bool TryGetGenericContextArguments( ContextKey context, out ContextKey[] genericArgs )
        {
            if( _selectors.TryGetValue( context.ID, out var selector ) && selector is UniformSelector uniform )
            {
                genericArgs = uniform.Contexts;
                return true;
            }
            genericArgs = null;
            return false;
        }

        // --- Compatibility Helpers ---

        /// <summary>
        /// Helper for descriptors that don't support dynamic resolution yet (legacy).
        /// Returns the Uniform arguments if available.
        /// </summary>
        [Obsolete( "Use Resolve with a proper IContextSelector for dynamic resolution instead of fixed arguments" )]
        public static ContextKey[] GetContextArguments( ContextKey context )
        {
            if( _selectors.TryGetValue( context.ID, out var selector ) && selector is UniformSelector uniform )
            {
                return uniform.Contexts;
            }
            return Array.Empty<ContextKey>();
        }

        [Obsolete( "Use Resolve with a proper IContextSelector for dynamic resolution instead of fixed arguments" )]
        public static ContextKey GetCollectionElementContext( ContextKey containerContext )
        {
            var args = GetContextArguments( containerContext );
            return args.Length > 0 ? args[0] : ContextIDs.Default;
        }

        [Obsolete( "Use Resolve with a proper IContextSelector for dynamic resolution instead of fixed arguments" )]
        public static (ContextKey keyCtx, ContextKey valCtx) GetDictionaryElementContexts( ContextKey containerContext )
        {
            var args = GetContextArguments( containerContext );
            if( args.Length >= 2 ) return (args[0], args[1]);
            return (ContextIDs.Default, ContextIDs.Default);
        }

        /// <summary>
        /// Gets the unique ID for a context type.
        /// </summary>
        public static ContextKey GetID( Type contextType )
        {
            if( contextType == null ) return ContextIDs.Default;

            if( _typeToId.TryGetValue( contextType, out int id ) )
                return new ContextKey( id );

            if( typeof( Ctx.Asset ).IsAssignableFrom( contextType ) )
            {
                Register( contextType, ContextIDs.Asset );
                return ObjectContext.Asset;
            }
            if( typeof( Ctx.Ref ).IsAssignableFrom( contextType ) )
            {
                Register( contextType, ContextIDs.Ref );
                return ObjectContext.Ref;
            }
            if( typeof( Ctx.Value ).IsAssignableFrom( contextType ) )
            {
                Register( contextType, ContextIDs.Default );
                return ObjectContext.Default;
            }

            if( contextType.IsGenericType && typeof( Ctx.IContext ).IsAssignableFrom( contextType ) )
            {
                if( TryProcessGenericType( contextType, contextType, out id ) )
                {
                    Register( contextType, id );
                    return new ContextKey( id );
                }
            }

            var interfaces = contextType.GetInterfaces();

            foreach( var i in interfaces )
            {
                if( !typeof( Ctx.IContext ).IsAssignableFrom( i ) ) continue;
                if( i == typeof( Ctx.IContext ) ) continue;

                if( i.IsGenericType )
                {
                    if( TryProcessGenericType( i, contextType, out id ) )
                        return new ContextKey( id );
                }
                else
                {
                    ContextKey aliasId = GetID( i );
                    if( aliasId.ID != ContextIDs.Default )
                    {
                        Register( contextType, aliasId.ID );
                        return aliasId;
                    }
                }
            }

            if( contextType.IsInterface && typeof( Ctx.IContext ).IsAssignableFrom( contextType ) && contextType != typeof( Ctx.IContext ) )
            {
                id = _nextDynamicId++;
                Register( contextType, id );
                return new ContextKey( id );
            }

            return ContextIDs.Default;
        }

        private static bool TryProcessGenericType( Type genericInterface, Type originalType, out int id )
        {
            if( !typeof( Ctx.IContext ).IsAssignableFrom( genericInterface ) )
            {
                id = 0;
                return false;
            }

            Type genericDef = genericInterface.GetGenericTypeDefinition();
            Type[] typeArgs = genericInterface.GetGenericArguments();

            foreach( var arg in typeArgs )
            {
                if( arg.IsGenericParameter )
                {
                    id = 0;
                    return false;
                }
            }

            List<ContextKey> contextArgs = new List<ContextKey>();
            bool hasValidArgs = false;

            foreach( var arg in typeArgs )
            {
                if( typeof( Ctx.IContext ).IsAssignableFrom( arg ) )
                {
                    contextArgs.Add( GetID( arg ) );
                    hasValidArgs = true;
                }
            }

            if( hasValidArgs )
            {
                id = GetOrRegisterGenericContext( genericDef, contextArgs.ToArray(), originalType ).ID;
                return true;
            }

            if( originalType == genericInterface )
            {
                id = 0;
                return false;
            }

            id = GetID( genericInterface ).ID;
            return true;
        }

        public static ContextKey GetOrRegisterGenericContext( Type genericDefinition, ContextKey[] args, Type sourceContextType = null )
        {
            StringBuilder keyBuilder = new StringBuilder();
            keyBuilder.Append( genericDefinition.AssemblyQualifiedName );
            keyBuilder.Append( '[' );
            for( int i = 0; i < args.Length; i++ )
            {
                if( i > 0 ) keyBuilder.Append( ',' );
                keyBuilder.Append( args[i].ID );
            }
            keyBuilder.Append( ']' );

            string key = keyBuilder.ToString();

            if( !_genericCombinations.TryGetValue( key, out int id ) )
            {
                id = _nextDynamicId++;
                _genericCombinations[key] = id;

                // Register uniform selector for generic args
                RegisterContextArguments( new ContextKey( id ), args );
            }

            if( sourceContextType != null )
            {
                Register( sourceContextType, id );
            }

            return new ContextKey( id );
        }
    }
}