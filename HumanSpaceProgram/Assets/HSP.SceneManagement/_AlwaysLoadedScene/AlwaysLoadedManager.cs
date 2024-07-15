using HSP.Core.Mods;
using HSP.Core.SceneManagement;
using HSP.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Core
{
    /// <summary>
    /// A manager that is loaded immediately and remains loaded until the game is exited.
    /// </summary>
    public class AlwaysLoadedManager : SingletonMonoBehaviour<AlwaysLoadedManager>
    {
        public const string ALWAYS_LOADED_SCENE_NAME = "_AlwaysLoaded";

        public static AlwaysLoadedManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
#warning TODO - finish here. !!!!!!!!!!!!!!!!!
            // Load mods before caching autorunning methods.
            // Because mods might (WILL and SHOULD) attach autorunning methods via the attributes.
           // HumanSpaceProgramMods.LoadModAssemblies();

            HSPEventListenerAttribute.CreateEventsForAutorunningMethods( AppDomain.CurrentDomain.GetAssemblies() );

            // Invoke after mods are loaded (because mods may want use it).
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_IMMEDIATELY );
        }

        void Start()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_EARLY );

            //SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null );
            SceneLoader.LoadSceneAsync( "MainMenu", true, false, null );
        }
    }
}