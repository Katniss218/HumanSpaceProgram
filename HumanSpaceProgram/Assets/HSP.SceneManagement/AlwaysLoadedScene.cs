using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    /// <summary>
    /// This class is loaded at the start of the game and remains loaded until the game is exited.
    /// </summary>
    public sealed class AlwaysLoadedScene : SingletonMonoBehaviour<AlwaysLoadedScene>
    {
        public static string UNITY_SCENE_NAME { get => "_AlwaysLoaded_"; }

        private UnityEngine.SceneManagement.Scene _unityScene;
        public UnityEngine.SceneManagement.Scene UnityScene
        {
            get
            {
                if( !_unityScene.IsValid() )
                {
                    _unityScene = SceneManager.GetSceneByName( UNITY_SCENE_NAME );
                }
                if( !_unityScene.isLoaded )
                {
                    throw new InvalidOperationException( $"The '{UNITY_SCENE_NAME}' scene is not loaded. This scene should be the first scene that is loaded and should remain loaded at all times." );
                }

                return _unityScene;
            }
        }

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