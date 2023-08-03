using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.OverridableEvents;

namespace KSS.Core
{
    public static class HSPOverridableEvent
    {
        /// <summary>
        /// The static event manager for the entire Human Space Program.
        /// </summary>
        /// <remarks>
        /// Used for a variety of actions relating to the game, loading scenes, and creating things.
        /// </remarks>
        public static OverridableEventManager<object> EventManager { get; private set; } = new OverridableEventManager<object>();

        /// <summary>
        /// The identifier of the vanilla namespace. Use this to avoid magic strings.
        /// </summary>
        public const string NAMESPACE_VANILLA = "vanilla";

        /// <summary>
        /// Invoked at the immediate start of the game. This is always the first invoked event.
        /// </summary>
        public const string STARTUP_IMMEDIATELY = NAMESPACE_VANILLA + ".startup.immediately";
        /// <summary>
        /// Invoked immediately after loading the main menu scene.
        /// </summary>
        public const string STARTUP_MAINMENU = NAMESPACE_VANILLA + ".startup.mainmenu";
        /// <summary>
        /// Invoked immediately after loading the gameplay scene.
        /// </summary>
        public const string STARTUP_GAMEPLAY = NAMESPACE_VANILLA + ".startup.gameplay";

        /// <summary>
        /// Invoked to create the timeline loader.
        /// </summary>
        public const string TIMELINE_LOADER_CREATE = NAMESPACE_VANILLA + ".timeline.loader.create";
        /// <summary>
        /// Invoked to create the timeline saver.
        /// </summary>
        public const string TIMELINE_SAVER_CREATE = NAMESPACE_VANILLA + ".timeline.loader.create";
    }
}
