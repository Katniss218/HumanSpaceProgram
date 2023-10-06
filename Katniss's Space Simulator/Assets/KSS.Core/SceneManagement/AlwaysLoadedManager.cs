using KSS.Core.Mods;
using KSS.Core.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A manager that is loaded immediately and remains loaded until the game is exited.
    /// </summary>
    public class AlwaysLoadedManager : MonoBehaviour
    {
        public const string ALWAYS_LOADED_SCENE_NAME = "_AlwaysLoaded";

        void Awake()
        {
            // Load mods before caching autorunning methods.
            // Because mods might (will / should) use autorunning methods via the attributes.
            ModLoader.LoadModAssemblies();

            HSPEventListenerAttribute.CreateEventsForAutorunningMethods( AppDomain.CurrentDomain.GetAssemblies() );

            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_IMMEDIATELY );
        }

        void Start()
        {
            SceneLoader.LoadSceneAsync( "MainMenu", true, false, null );
        }

        internal static GameObject[] GetAllManagerGameObjects()
        {
            return new GameObject[] { };
        }
    }
}