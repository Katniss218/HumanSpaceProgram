using KSS.Core.Mods;
using KSS.Core.SceneManagement;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core
{
    /// <summary>
    /// A manager that is loaded immediately and remains loaded until the game is exited.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
    public class AlwaysLoadedManager : SingletonMonoBehaviour<AlwaysLoadedManager>
    {
        public const string ALWAYS_LOADED_SCENE_NAME = "_AlwaysLoaded";

        public static AlwaysLoadedManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

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
            SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null );
        }

        //
        //      SERIALIZATION OF MANAGERS - this can be moved to its own class.
        //

        private static readonly JsonPreexistingGameObjectsStrategy _managersStrat = new JsonPreexistingGameObjectsStrategy( GetAllManagerGameObjects );

        private static GameObject[] GetAllManagerGameObjects()
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
        private static void OnBeforeSave( object ee )
        {
            var e = (TimelineManager.SaveEventData)ee;

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay" ) );
            _managersStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "objects.json" );
            _managersStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "data.json" );
            e.objectActions.Add( _managersStrat.SaveAsync_Object );
            e.dataActions.Add( _managersStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_managers" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay" ) );
            _managersStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "objects.json" );
            _managersStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "data.json" );
            e.objectActions.Add( _managersStrat.LoadAsync_Object );
            e.dataActions.Add( _managersStrat.LoadAsync_Data );
        }
    }
}