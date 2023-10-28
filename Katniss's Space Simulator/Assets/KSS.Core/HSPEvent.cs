using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.OverridableEvents;

namespace KSS.Core
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
        public static OverridableEventManager<object> EventManager { get; private set; } = new OverridableEventManager<object>();

        // TO DEVELOPERS:
        // - Every vanilla game event should have a public constant here.

        /// <summary>
        /// The identifier of the vanilla namespace. Use this to avoid magic strings.
        /// </summary>
        public const string NAMESPACE_VANILLA = "vanilla";

        //

        /// <summary>
        /// Invoked at the immediate start of the game. This event is always invoked first.
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
        /// Invoked immediately after loading the design scene.
        /// </summary>
        public const string STARTUP_DESIGN = NAMESPACE_VANILLA + ".startup.design";
        
        /// <summary>
        /// Invoked immediately after loading the editor scene.
        /// </summary>
        public const string STARTUP_EDITOR = NAMESPACE_VANILLA + ".startup.editor";

        //

        /// <summary>
        /// Invoked when the player toggles the escape (pause) menu in the gameplay scene.
        /// </summary>
        public const string ESCAPE_MAINMENU = NAMESPACE_VANILLA + ".escape.mainmenu";
        
        /// <summary>
        /// Invoked when the player toggles the escape (pause) menu in the gameplay scene.
        /// </summary>
        public const string ESCAPE_GAMEPLAY = NAMESPACE_VANILLA + ".escape.gameplay";
        
        /// <summary>
        /// Invoked when the player toggles the escape (pause) menu in the design scene.
        /// </summary>
        public const string ESCAPE_DESIGN = NAMESPACE_VANILLA + ".escape.design";
        
        /// <summary>
        /// Invoked when the player toggles the escape (pause) menu in the design scene.
        /// </summary>
        public const string ESCAPE_EDITOR = NAMESPACE_VANILLA + ".escape.editor";

        //

        /// <summary>
        /// Invoked before loading a new game state (timeline + save).
        /// </summary>
        public const string TIMELINE_BEFORE_LOAD = NAMESPACE_VANILLA + ".timeline.load.before";

        /// <summary>
        /// Invoked after loading a new game state (timeline + save).
        /// </summary>
        public const string TIMELINE_AFTER_LOAD = NAMESPACE_VANILLA + ".timeline.load.after";

        /// <summary>
        /// Invoked before saving the current game state (timeline + save).
        /// </summary>
        public const string TIMELINE_BEFORE_SAVE = NAMESPACE_VANILLA + ".timeline.save.before";

        /// <summary>
        /// Invoked after saving the current game state (timeline + save).
        /// </summary>
        public const string TIMELINE_AFTER_SAVE = NAMESPACE_VANILLA + ".timeline.save.after";

        /// <summary>
        /// Invoked before creating a new game state (timeline + default save).
        /// </summary>
        public const string TIMELINE_BEFORE_NEW = NAMESPACE_VANILLA + ".timeline.new.before";

        /// <summary>
        /// Invoked after creating a new game state (timeline + default save).
        /// </summary>
        public const string TIMELINE_AFTER_NEW = NAMESPACE_VANILLA + ".timeline.new.after";
    }
}