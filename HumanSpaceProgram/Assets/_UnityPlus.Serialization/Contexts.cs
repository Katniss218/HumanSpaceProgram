using System;

namespace UnityPlus.Serialization
{
    namespace Ctx
    {
        /// <summary>
        /// Base interface for all context marker types.
        /// </summary>
        public interface IContext { }

        // --- Core Markers ---

        public interface Value : IContext { }
        public interface Asset : IContext { }
        public interface Ref : IContext { }

        // --- Generics ---

        /// <summary>
        /// Creates a context for a dictionary where keys use TKeyContext and values use TValueContext.
        /// Extend this interface to create a shorthand alias.
        /// </summary>
        public interface KeyValue<TKeyContext, TValueContext> : IContext where TKeyContext : IContext where TValueContext : IContext { }

        /// <summary>
        /// Creates a context for a collection where elements use TElementContext.
        /// Extend this interface to create a shorthand alias.
        /// </summary>
        public interface Array<TElementContext> : IContext where TElementContext : IContext { }
    }

    // Implementers are encouraged to create their own context marker types in an analogous way when necessary.

    /// <summary>
    /// Contains raw integer constants for Contexts. Used primarily for Attributes.
    /// </summary>
    internal static class ContextIDs
    {
        // v3 compat only requires the class name / etc to match in the memberwise mapping's Withmember fluent method.
        // this is done by the rest of the ...Context static classes below.
        public const int Default = 0;
        public const int Value = Default;
        public const int Ref = 536806356;
        public const int Asset = 271721118;

        public const int ArrayDefault = Default;
        public const int ArrayValues = Default;
        public const int ArrayRefs = 429303064;
        public const int ArrayAssets = -261997342;

        public const int DictDefault = Default;
        public const int DictValueToValue = Default;
        public const int DictValueToRef = 846497468;
        public const int DictRefToValue = 132031121;
        public const int DictRefToRef = 240733231;
        public const int DictValueToAsset = 172983851;
        public const int DictRefToAsset = -526574830;
    }

    /// <summary>
    /// General contexts applicable to any object type.
    /// Includes backward compatibility constants and Type-to-Int mapping initialization.
    /// </summary>
    [Obsolete( "Use the `IContext` marker interface in production environments (with typeof). This class exists for backwards compatibility only." )]
    public static class ObjectContext
    {
        public const int Default = ContextIDs.Default;
        public const int Value = Default;
        public const int Ref = ContextIDs.Ref;
        public const int Asset = ContextIDs.Asset;

        static ObjectContext()
        {
            // Map the Types to the Legacy Ints
            ContextRegistry.Register( typeof( Ctx.Value ), ContextIDs.Default );
            ContextRegistry.Register( typeof( Ctx.Asset ), ContextIDs.Asset );
            ContextRegistry.Register( typeof( Ctx.Ref ), ContextIDs.Ref );

            // Register Names
            ContextRegistry.RegisterName( ContextIDs.Default, "Default" );
            ContextRegistry.RegisterName( ContextIDs.Ref, "Reference" );
            ContextRegistry.RegisterName( ContextIDs.Asset, "Asset" );
        }
    }

    [Obsolete( "Use the `IContext` marker interface in production environments (with typeof). This class exists for backwards compatibility only." )]
    public static class ArrayContext
    {
        public const int Default = ObjectContext.Default;
        public const int Values = Default;
        public const int Refs = ContextIDs.ArrayRefs;
        public const int Assets = ContextIDs.ArrayAssets;

        static ArrayContext()
        {
            // Map legacy array contexts to the new Unified Collection Context type
            ContextRegistry.Register( typeof( Ctx.Array<Ctx.Ref> ), ContextIDs.ArrayRefs );
            ContextRegistry.Register( typeof( Ctx.Array<Ctx.Asset> ), ContextIDs.ArrayAssets );

            // Explicitly register the Rules for these integer IDs
            ContextRegistry.RegisterContextArguments( new ContextKey( Refs ), ObjectContext.Ref );
            ContextRegistry.RegisterContextArguments( new ContextKey( Assets ), ObjectContext.Asset );

            ContextRegistry.RegisterName( ContextIDs.ArrayRefs, "Array<Ref>" );
            ContextRegistry.RegisterName( ContextIDs.ArrayAssets, "Array<Asset>" );
        }
    }

    [Obsolete( "Use the `IContext` marker interface in production environments (with typeof). This class exists for backwards compatibility only." )]
    public static class KeyValueContext
    {
        // Legacy constants kept for compatibility, but internally mapped via CtxDict<,> logic if used.
        public const int Default = ObjectContext.Default;
        public const int ValueToValue = Default;

        public const int ValueToRef = ContextIDs.DictValueToRef;
        public const int RefToValue = ContextIDs.DictRefToValue;
        public const int RefToRef = ContextIDs.DictRefToRef;
        public const int ValueToAsset = ContextIDs.DictValueToAsset;
        public const int RefToAsset = ContextIDs.DictRefToAsset;

        static KeyValueContext()
        {
            // We map the legacy combination IDs to the equivalent Generic Type combination.
            ContextRegistry.Register( typeof( Ctx.KeyValue<Ctx.Value, Ctx.Ref> ), ContextIDs.DictValueToRef );
            ContextRegistry.Register( typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Value> ), ContextIDs.DictRefToValue );
            ContextRegistry.Register( typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Ref> ), ContextIDs.DictRefToRef );
            ContextRegistry.Register( typeof( Ctx.KeyValue<Ctx.Value, Ctx.Asset> ), ContextIDs.DictValueToAsset );
            ContextRegistry.Register( typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Asset> ), ContextIDs.DictRefToAsset );

            // Explicitly register the Rules for these integer IDs
            ContextRegistry.RegisterContextArguments( new ContextKey( ValueToRef ), ObjectContext.Default, ObjectContext.Ref );
            ContextRegistry.RegisterContextArguments( new ContextKey( RefToValue ), ObjectContext.Ref, ObjectContext.Default );
            ContextRegistry.RegisterContextArguments( new ContextKey( RefToRef ), ObjectContext.Ref, ObjectContext.Ref );
            ContextRegistry.RegisterContextArguments( new ContextKey( ValueToAsset ), ObjectContext.Default, ObjectContext.Asset );
            ContextRegistry.RegisterContextArguments( new ContextKey( RefToAsset ), ObjectContext.Ref, ObjectContext.Asset );

            ContextRegistry.RegisterName( ContextIDs.DictValueToRef, "Dict<Default, Ref>" );
            ContextRegistry.RegisterName( ContextIDs.DictRefToValue, "Dict<Ref, Default>" );
            ContextRegistry.RegisterName( ContextIDs.DictRefToRef, "Dict<Ref, Ref>" );
            ContextRegistry.RegisterName( ContextIDs.DictValueToAsset, "Dict<Default, Asset>" );
            ContextRegistry.RegisterName( ContextIDs.DictRefToAsset, "Dict<Ref, Asset>" );
        }
    }
}