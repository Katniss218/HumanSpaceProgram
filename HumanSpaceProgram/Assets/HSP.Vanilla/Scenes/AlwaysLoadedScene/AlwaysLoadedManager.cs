using HSP.Content.Mods;
using HSP.SceneManagement;
using HSP.Vanilla.Scenes.MainMenuScene;
using System;
using UnityEngine;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    /// <summary>
    /// Invoked at the immediate start of the game. This event is always invoked first.
    /// </summary>
    public static class HSPEvent_STARTUP_IMMEDIATELY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.immediately";
    }

    /// <summary>
    /// Invoked just before loading the main menu scene, after the immediate startup.
    /// </summary>
    public static class HSPEvent_STARTUP_EARLY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.early";
    }

    /// <summary>
    /// A manager that is loaded immediately and remains loaded until the game is exited.
    /// </summary>
    public class AlwaysLoadedManager : SingletonMonoBehaviour<AlwaysLoadedManager>
    {
        public const string ALWAYS_LOADED_SCENE_NAME = "_AlwaysLoaded";

        public static AlwaysLoadedManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        public bool LoadMainMenu { get; set; } = true;

        void Awake()
        {
            // Load mods before caching autorunning methods.
            // Because mods might (WILL and SHOULD) attach autorunning methods via the attributes.
            HumanSpaceProgramModLoader.LoadModAssemblies();

            HSPEventListenerAttribute.CreateEventsForAutorunningMethods( AppDomain.CurrentDomain.GetAssemblies() );

            // Invoke after mods are loaded (because mods may want use it).
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_IMMEDIATELY.ID );
        }

        void Start()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_EARLY.ID );

            if( LoadMainMenu )
            {
                SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null );
            }
        }
    }
}