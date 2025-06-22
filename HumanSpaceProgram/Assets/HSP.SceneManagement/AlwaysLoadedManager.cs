using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HSP.SceneManagement
{
    /// <summary>
    /// A manager that is loaded immediately and remains active until the game is exited.
    /// </summary>
    /// <remarks>
    /// You can use this gameobject to set up logic that needs to be updated, or be a monobehariour.
    /// </remarks>
    public class AlwaysLoadedManager : HSPSceneManager<AlwaysLoadedManager>
    {
        public static new string UNITY_SCENE_NAME => "_AlwaysLoaded";

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

        protected override void OnActivate()
        {
        }

        protected override void OnDeactivate()
        {
        }

        protected override void OnLoad()
        {
        }

        protected override void OnUnload()
        {
        }
    }
}