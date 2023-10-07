using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.OverridableEvents;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Manages the currently loaded timeline (save/workspace). See <see cref="TimelineMetadata"/> and <see cref="SaveMetadata"/>.
    /// </summary>
    public class TimelineManager : MonoBehaviour
    {
        #region SINGLETON UGLINESS
        private static TimelineManager ___instance;
        public static TimelineManager Instance
        {
            get
            {
                if( ___instance == null )
                {
                    ___instance = FindObjectOfType<TimelineManager>();
                }
                return ___instance;
            }
        }
        #endregion

        static readonly JsonPreexistingGameObjectsStrategy _managersStrat = new JsonPreexistingGameObjectsStrategy( AlwaysLoadedManager.GetAllManagerGameObjects );
        static readonly JsonPreexistingGameObjectsStrategy _celestialBodiesStrat = new JsonPreexistingGameObjectsStrategy( CelestialBodyManager.GetAllRootGameObjects );
        static readonly JsonExplicitHierarchyGameObjectsStrategy _vesselsStrat = new JsonExplicitHierarchyGameObjectsStrategy( VesselManager.GetAllRootGameObjects );

        /// <summary>
        /// The save file version to use when creating new save files.
        /// </summary>
        public static readonly SaveVersion CURRENT_SAVE_VERSION = new SaveVersion( 0, 0 );

        /// <summary>
        /// Checks if a timeline is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading =>
                (_saver != null && _saver.CurrentState != ISaver.State.Idle)
             || (_loader != null && _loader.CurrentState != ILoader.State.Idle);

        public static TimelineMetadata CurrentTimeline { get; private set; }

        static AsyncSaver _saver;
        static AsyncLoader _loader;

        static bool _wasPausedBeforeSerializing = false;

        public static void SerializationPauseFunc()
        {
            _wasPausedBeforeSerializing = TimeManager.IsPaused;
            TimeManager.Pause();
            TimeManager.LockTimescale = true;
        }

        public static void SerializationUnpauseFunc()
        {
            if( !_wasPausedBeforeSerializing )
            {
#warning TODO - doesn't unpause - something else sets the "old" timescale.
                TimeManager.Unpause();
            }
            TimeManager.LockTimescale = false;
        }

        static void CreateDefaultSaver()
        {
            _saver = new AsyncSaver(
                SerializationPauseFunc, SerializationUnpauseFunc,
                new Func<ISaver, IEnumerator>[] { _vesselsStrat.Save_Object },
                new Func<ISaver, IEnumerator>[] { _managersStrat.Save_Data, _celestialBodiesStrat.Save_Data, _vesselsStrat.Save_Data }
            );
        }

        static void CreateDefaultLoader()
        {
            _loader = new AsyncLoader(
                SerializationPauseFunc, SerializationUnpauseFunc,
                new Func<ILoader, IEnumerator>[] { _managersStrat.Load_Object, _celestialBodiesStrat.Load_Object, _vesselsStrat.Load_Object },
                new Func<ILoader, IEnumerator>[] { _managersStrat.Load_Data, _celestialBodiesStrat.Load_Data, _vesselsStrat.Load_Data }
            );
        }

        static void EnsureDirectory( string path )
        {
            if( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }
        }

        static void SetStratPaths( string timelineId, string saveId )
        {
            EnsureDirectory( SaveMetadata.GetRootDirectory( timelineId, saveId ) );

            EnsureDirectory( Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "Vessels" ) );
            _vesselsStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "Vessels", "objects.json" );
            _vesselsStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "Vessels", "data.json" );
            EnsureDirectory( Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "CelestialBodies" ) );
            _celestialBodiesStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "CelestialBodies", "objects.json" );
            _celestialBodiesStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "CelestialBodies", "data.json" );
            EnsureDirectory( Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "Gameplay" ) );
            _managersStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "Gameplay", "objects.json" );
            _managersStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "Gameplay", "data.json" );
        }

        /// <summary>
        /// Asynchronously saves the current game state over multiple frames. <br/>
        /// The game should remain paused for the duration of the saving (this is generally handled automatically, but be careful).
        /// </summary>
        public static void BeginSaveAsync( string timelineId, string saveId )
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start saving while already saving/loading." );
            }

            CreateDefaultSaver();

            SetStratPaths( timelineId, saveId );
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_SAVE, _saver );

            // write timeline.json to disk
            // write save.json to disk.
            _saver.SaveAsync( Instance );
            SaveMetadata savedSave = new SaveMetadata( timelineId, saveId );
            savedSave.SaveVersion = CURRENT_SAVE_VERSION;
            savedSave.WriteToDisk();

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_SAVE, _saver );
        }

        /// <summary>
        /// Asynchronously loads the saved game state over multiple frames. <br/>
        /// The game should remain paused for the duration of the loading (this is generally handled automatically, but be careful).
        /// </summary>
        public static void BeginLoadAsync( string timelineId, string saveId )
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start loading while already saving/loading." );
            }

            TimelineMetadata loadedTimeline = new TimelineMetadata( timelineId );
            SaveMetadata loadedSave = new SaveMetadata( timelineId, saveId );
#warning TODO - load timeline's and save's metadata too.

            CreateDefaultLoader();

            SetStratPaths( timelineId, saveId );
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_LOAD, _loader );

            _loader.LoadAsync( Instance );
            CurrentTimeline = loadedTimeline;

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_LOAD, _loader );
        }

        /// <summary>
        /// Creates a new default (empty) timeline and "loads" it.
        /// </summary>
        public static void CreateNew( string timelineId, string saveId )
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start loading while already saving/loading." );
            }

            TimelineMetadata newTimeline = new TimelineMetadata( timelineId );

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_NEW );

            CurrentTimeline = newTimeline;
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_NEW );
        }
    }
}