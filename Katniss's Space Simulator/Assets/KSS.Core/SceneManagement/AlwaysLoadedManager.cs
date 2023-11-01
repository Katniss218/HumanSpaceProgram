using KSS.Core.Mods;
using KSS.Core.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;
using KSS.Core.Serialization;
using System.IO;
using UnityPlus.Serialization.Strategies;

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
            SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null );
        }


        private static readonly JsonPreexistingGameObjectsStrategy _managersStrat = new JsonPreexistingGameObjectsStrategy( GetAllManagerGameObjects );

        private static GameObject[] GetAllManagerGameObjects()
        {
            // An alternative approach could be to have a layer for manager objects (canonically a single object for all tho).

            HSPManager[] managers =  FindObjectsOfType<HSPManager>();
            List<GameObject> gameObjects = new List<GameObject>();

            foreach( var manager in managers )
            {
                if( gameObjects.Contains( manager.gameObject ) )
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

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay" ) );
            _managersStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "objects.json" );
            _managersStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "data.json" );
            e.objectActions.Add( _managersStrat.SaveAsync_Object );
            e.dataActions.Add( _managersStrat.SaveAsync_Data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_BEFORE_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_managers" )]
        private static void OnBeforeLoad( object ee )
        {
            var e = (TimelineManager.LoadEventData)ee;

            TimelineManager.EnsureDirectoryExists( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay" ) );
            _managersStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "objects.json" );
            _managersStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Gameplay", "data.json" );
            e.objectActions.Add( _managersStrat.LoadAsync_Object );
            e.dataActions.Add( _managersStrat.LoadAsync_Data );
        }
    }
}