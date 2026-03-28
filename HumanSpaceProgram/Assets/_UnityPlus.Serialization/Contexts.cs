using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Base interface for all context marker types.
    /// </summary>
    public interface IContext { }

    public static class Ctx
    {
        // --- Core Markers ---

        /// <summary>
        /// The default context for general-purpose serialization. Used when no specific context is specified.
        /// </summary>
        public interface Value : IContext { }
        /// <summary>
        /// Looks up the object in the <see cref="AssetManagement.AssetRegistry"/> via an asset ID. Produces a { "$assetref": "asset_id" } structure.
        /// </summary>
        public interface Asset : IContext { }
        /// <summary>
        /// Serializes the object as a reference to a different instance, rather than by value. Produces a { "$ref": "guid" } structure.
        /// </summary>
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
        public const int Default = ContextIDs.Default; // Use typeof( Ctx.Value ) in new code.
        public const int Value = Default;
        public const int Ref = ContextIDs.Ref; // Use typeof( Ctx.Ref ) in new code.
        public const int Asset = ContextIDs.Asset; // Use typeof( Ctx.Asset ) in new code.
    }

    [Obsolete( "Use the `IContext` marker interface in production environments (with typeof). This class exists for backwards compatibility only." )]
    public static class ArrayContext
    {
        public const int Default = ObjectContext.Default; // Use typeof( Ctx.Array<Ctx.Value> ) in new code.
        public const int Values = Default;
        public const int Refs = ContextIDs.ArrayRefs; // Use typeof( Ctx.Array<Ctx.Ref> ) in new code.
        public const int Assets = ContextIDs.ArrayAssets; // Use typeof( Ctx.Array<Ctx.Asset> ) in new code.
    }

    [Obsolete( "Use the `IContext` marker interface in production environments (with typeof). This class exists for backwards compatibility only." )]
    public static class KeyValueContext
    {
        // Legacy constants kept for compatibility, but internally mapped via CtxDict<,> logic if used.
        public const int Default = ObjectContext.Default; // Use typeof( Ctx.KeyValue<Ctx.Value, Ctx.Value> ) in new code.
        public const int ValueToValue = Default;

        public const int ValueToRef = ContextIDs.DictValueToRef; // Use typeof( Ctx.KeyValue<Ctx.Value, Ctx.Ref> ) in new code.
        public const int RefToValue = ContextIDs.DictRefToValue; // Use typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Value> ) in new code.
        public const int RefToRef = ContextIDs.DictRefToRef; // Use typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Ref> ) in new code.
        public const int ValueToAsset = ContextIDs.DictValueToAsset; // Use typeof( Ctx.KeyValue<Ctx.Value, Ctx.Asset> ) in new code.
        public const int RefToAsset = ContextIDs.DictRefToAsset; // Use typeof( Ctx.KeyValue<Ctx.Ref, Ctx.Asset> ) in new code.
    }
}