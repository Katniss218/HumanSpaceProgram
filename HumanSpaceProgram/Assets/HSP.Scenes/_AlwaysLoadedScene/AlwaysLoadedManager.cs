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
            // Load mods before caching autorunning methods.
            // Because mods might (WILL and SHOULD) attach autorunning methods via the attributes.
            HumanSpaceProgramMods.LoadModAssemblies();

            HSPEventListenerAttribute.CreateEventsForAutorunningMethods( AppDomain.CurrentDomain.GetAssemblies() );

            // Invoke after mods are loaded (because mods may want use it).
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_IMMEDIATELY );
        }

        void Start()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent.STARTUP_EARLY );

            SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null );
        }

        //
        //      SERIALIZATION OF MANAGERS - this can be moved to its own class.
        //


        /*private static GameObject[] GetAllManagerGameObjects()
        {
            PreexistingReference[] managers = FindObjectsOfType<PreexistingReference>();
            HashSet<GameObject> gameObjects = new HashSet<GameObject>();

            foreach( var manager in managers )
            {
                if( manager.gameObject.layer != (int)Layer.MANAGERS )
                {
                    continue;
                }

                gameObjects.Add( manager.gameObject );
            }

            return gameObjects.ToArray();
        }
        
        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_managers" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay" ) );
            JsonSerializedDataHandler _managersDataHandler = new JsonSerializedDataHandler( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "managers.json" ) );

            SerializationUnit _managersStrat = new SerializationUnit( _managersDataHandler, GetAllManagerGameObjects );
            e.objectActions.Add( _managersStrat.SaveAsync_Object );
            e.dataActions.Add( _managersStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_managers" )]
        private static void OnBeforeLoad( TimelineManager.LoadEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay" ) );
            JsonSerializedDataHandler _managersDataHandler = new JsonSerializedDataHandler( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "managers.json" ) );

            SerializationUnit _managersStrat = new SerializationUnit( _managersDataHandler, GetAllManagerGameObjects );
            e.objectActions.Add( _managersStrat.LoadAsync_Object );
            e.dataActions.Add( _managersStrat.LoadAsync_Data );
        }*/
    }
}