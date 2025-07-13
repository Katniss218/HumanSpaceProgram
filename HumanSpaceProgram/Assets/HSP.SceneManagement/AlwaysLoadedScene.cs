using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HSP.SceneManagement
{
    /// <summary>
    /// This class is loaded at the start of the game and remains loaded until the game is exited.
    /// </summary>
    public sealed class AlwaysLoadedScene : SingletonMonoBehaviour<AlwaysLoadedScene>
    {
        // This is not an HSPScene<T> because it's part of the scene management itself
        //   and shouldn't be able to be unloaded or treated like a scene.

        public static AlwaysLoadedScene Instance => instance;
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