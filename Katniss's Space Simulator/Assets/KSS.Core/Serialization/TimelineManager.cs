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

        static readonly JsonPreexistingGameObjectsStrategy _managersStrat = new JsonPreexistingGameObjectsStrategy();
        static readonly JsonPreexistingGameObjectsStrategy _celestialBodiesStrat = new JsonPreexistingGameObjectsStrategy();
        static readonly JsonExplicitHierarchyGameObjectsStrategy _objectStrat = new JsonExplicitHierarchyGameObjectsStrategy();

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
                new Func<ISaver, IEnumerator>[] { _objectStrat.Save_Object },
                new Func<ISaver, IEnumerator>[] { _managersStrat.Save_Data, _celestialBodiesStrat.Save_Data, _objectStrat.Save_Data }
            );
        }

        static void CreateDefaultLoader()
        {
            _loader = new AsyncLoader(
                SerializationPauseFunc, SerializationUnpauseFunc,
                new Func<ILoader, IEnumerator>[] { _managersStrat.Load_Object, _celestialBodiesStrat.Load_Object, _objectStrat.Load_Object },
                new Func<ILoader, IEnumerator>[] { _objectStrat.Load_Data }
            );
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

            _objectStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "objects.json" );
            _objectStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "data.json" );
            if( !Directory.Exists( SaveMetadata.GetRootDirectory( timelineId, saveId ) ) )
            {
                Directory.CreateDirectory( SaveMetadata.GetRootDirectory( timelineId, saveId ) );
            }
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_SAVE, _saver );

            // write timeline.json to disk
            // write save.json to disk.
            _saver.SaveAsync( Instance );

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
#warning TODO - load timeline's metadata too.

            CreateDefaultLoader();

            _objectStrat.ObjectsFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "objects.json" );
            _objectStrat.DataFilename = Path.Combine( SaveMetadata.GetRootDirectory( timelineId, saveId ), "data.json" );
            if( !Directory.Exists( SaveMetadata.GetRootDirectory( timelineId, saveId ) ) )
            {
                Directory.CreateDirectory( SaveMetadata.GetRootDirectory( timelineId, saveId ) );
            }
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