﻿using UnityPlus.OverridableEvents;

namespace HSP.Core
{
    /// <summary>
    /// A container for the overridable event manager and builtin (vanilla) event identifiers.
    /// </summary>
    public static class HSPEvent
    {
#warning TODO - split these from a main class so that each 'module' (assembly) contains events relevant to it.
        /// <summary>
        /// The event manager for Human Space Program game (global) events.
        /// </summary>
        /// <remarks>
        /// Used for a variety of actions relating to the game, see the constants for an exhaustive list of vanilla events. <br/>
        /// TO MODDERS: Don't use it for events specific to some entity.
        /// </remarks>
        public static OverridableEventRegistry<object> EventManager { get; private set; } = new OverridableEventRegistry<object>();

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
        /// Invoked just before loading the main menu scene, after the immediate startup.
        /// </summary>
        public const string STARTUP_EARLY = NAMESPACE_VANILLA + ".startup.early";

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
        /// Invoked when the player toggles the escape (pause) menu in the editor scene.
        /// </summary>
        public const string ESCAPE_EDITOR = NAMESPACE_VANILLA + ".escape.editor";

        //

        /// <summary>
        /// Invoked before loading a new game state (timeline + save).
        /// </summary>
        public const string TIMELINE_BEFORE_LOAD = NAMESPACE_VANILLA + ".timeline.load.before";

        /// <summary>
        /// Invoked to load a new game state (timeline + save).
        /// </summary>
        public const string TIMELINE_LOAD = NAMESPACE_VANILLA + ".timeline.load";

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
        /// Invoked to save the current game state (timeline + save).
        /// </summary>
        public const string TIMELINE_SAVE = NAMESPACE_VANILLA + ".timeline.save";

        /// <summary>
        /// Invoked before creating a new game state (timeline + default save).
        /// </summary>
        public const string TIMELINE_BEFORE_NEW = NAMESPACE_VANILLA + ".timeline.new.before";

        /// <summary>
        /// Invoked after creating a new game state (timeline + default save).
        /// </summary>
        public const string TIMELINE_AFTER_NEW = NAMESPACE_VANILLA + ".timeline.new.after";

        /// <summary>
        /// Invoked to create a new game state (timeline + default save).
        /// </summary>
        public const string TIMELINE_NEW = NAMESPACE_VANILLA + ".timeline.new";

        // design scene runtime events.

        /// <summary>
        /// Invoked after the currently active tool in the design scene has changed.
        /// </summary>
        public const string DESIGN_AFTER_TOOL_CHANGED = NAMESPACE_VANILLA + ".designscene.tool.changed";

        /// <summary>
        /// Invoked before the vessel is loaded in the design scene.
        /// </summary>
        public const string DESIGN_BEFORE_LOAD = NAMESPACE_VANILLA + ".designscene.load.before";

        /// <summary>
        /// Invoked after the vessel is loaded in the design scene.
        /// </summary>
        public const string DESIGN_AFTER_LOAD = NAMESPACE_VANILLA + ".designscene.load.after";

        /// <summary>
        /// Invoked before the vessel is saved in the design scene.
        /// </summary>
        public const string DESIGN_BEFORE_SAVE = NAMESPACE_VANILLA + ".designscene.save.before";

        /// <summary>
        /// Invoked after the vessel is saved in the design scene.
        /// </summary>
        public const string DESIGN_AFTER_SAVE = NAMESPACE_VANILLA + ".designscene.save.after";

        // gameplay scene runtime events

        /// <summary>
        /// Invoked after the currently active tool in the gameplay scene has changed.
        /// </summary>
        public const string GAMEPLAY_AFTER_TOOL_CHANGED = NAMESPACE_VANILLA + ".gameplayscene.tool.changed";

        /// <summary>
        /// Invoked after the active object changes.
        /// </summary>
        public const string GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE = NAMESPACE_VANILLA + ".gameplayscene.after_activeobj_changed";

        /// <summary>
        /// Invoked after a vessel is first created and registered.
        /// </summary>
        public const string GAMEPLAY_AFTER_VESSEL_REGISTERED = NAMESPACE_VANILLA + ".gameplayscene.after_vessel_created";

        /// <summary>
        /// Invoked after a vessel is destroyed and unregistered.
        /// </summary>
        public const string GAMEPLAY_AFTER_VESSEL_UNREGISTERED = NAMESPACE_VANILLA + ".gameplayscene.after_vessel_destroyed";

        /// <summary>
        /// Invoked before the first camera starts rendering.
        /// </summary>
        public const string GAMEPLAY_BEFORE_RENDERING = NAMESPACE_VANILLA + ".gameplayscene.rendering.before";
        
        /// <summary>
        /// Invoked after the last camera has finished rendering.
        /// </summary>
        public const string GAMEPLAY_AFTER_RENDERING = NAMESPACE_VANILLA + ".gameplayscene.rendering.after";

        //public const string GAMEPLAY_CREATE_CAMERA = NAMESPACE_VANILLA + ".gameplayscene.create_camera_system";

        // ---
    }
}