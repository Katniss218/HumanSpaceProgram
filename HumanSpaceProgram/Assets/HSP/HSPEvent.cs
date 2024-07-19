using UnityPlus.OverridableEvents;

namespace HSP
{
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