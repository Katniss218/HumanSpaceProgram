using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
    /// Use this event to set up objects that should exist before everything else is loaded.
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
    /// A manager that is loaded immediately and remains active until the game is exited.
    /// </summary>
    /// <remarks>
    /// You can use this gameobject to set up logic that needs to be updated, or be a monobehariour.
    /// </remarks>
    public class AlwaysLoadedManager : SingletonMonoBehaviour<AlwaysLoadedManager>
    {
        public const string ALWAYS_LOADED_SCENE_NAME = "_AlwaysLoaded";

        public static AlwaysLoadedManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            HSPEventListenerAttribute.CreateEventsForAutorunningMethods( assemblies );

            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_LOAD_MOD_ASSEMBLIES.ID );

            assemblies = AppDomain.CurrentDomain.GetAssemblies() // Mod assemblies.
                .Except( assemblies );
            HSPEventListenerAttribute.CreateEventsForAutorunningMethods( assemblies );

            // Invoke after mods are loaded (because mods may want use it).
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_IMMEDIATELY.ID );
        }

        void Start()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_EARLY.ID );
        }
    }
}