using UnityPlus.OverridableEvents;

namespace HSP
{
    /// <summary>
    /// Invoked at the immediate start of the game, to load the mod assemblies (that have to be loaded before other events are invoked).
    /// </summary>
    public static class HSPEvent_STARTUP_LOAD_MOD_ASSEMBLIES
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.load_mod_assemblies";
    }

    /// <summary>
    /// Invoked immediately after loading the mod assemblies. <br/>
    /// Use this event to set up objects that should exist before everything else is loaded, and for other game initialization logic.
    /// </summary>
    public static class HSPEvent_STARTUP_IMMEDIATELY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.immediately";
    }

    /// <summary>
    /// Invoked after <see cref="HSPEvent_STARTUP_IMMEDIATELY"/>. <br/>
    /// Use this to load the main menu, and set up the deferred logic.
    /// </summary>
    public static class HSPEvent_STARTUP_EARLY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.early";
    }

    /// <summary>
    /// A container for the overridable event manager and builtin (vanilla) event identifiers.
    /// </summary>
    public static class HSPEvent
    {
        /// <summary>
        /// The event manager for Human Space Program game (global) events.
        /// </summary>
        /// <remarks>
        /// Used for a variety of actions relating to the game, see the constants for an exhaustive list of vanilla events. <br/>
        /// TO MODDERS: Don't use it for events specific to some entity.
        /// </remarks>
        public static OverridableEventRegistry<object> EventManager { get; private set; } = new OverridableEventRegistry<object>();

        /// <summary>
        /// The identifier of the vanilla namespace. Use this to avoid magic strings.
        /// </summary>
        public const string NAMESPACE_HSP = "hsp";
    }
}