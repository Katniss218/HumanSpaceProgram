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
    public class TimelineManager : SerializedManager
    {
        #region SINGLETON UGLINESS
        private static TimelineManager ___instance;
        private static TimelineManager instance
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

        public struct SaveEventData
        {
            public string timelineId;
            public string saveId;
            public List<Func<ISaver, IEnumerator>> objectActions;
            public List<Func<ISaver, IEnumerator>> dataActions;

            public SaveEventData( string timelineId, string saveId )
            {
                this.timelineId = timelineId;
                this.saveId = saveId;
                this.objectActions = new List<Func<ISaver, IEnumerator>>();
                this.dataActions = new List<Func<ISaver, IEnumerator>>();
            }
        }

        public struct LoadEventData
        {
            public string timelineId;
            public string saveId;
            public List<Func<ILoader, IEnumerator>> objectActions;
            public List<Func<ILoader, IEnumerator>> dataActions;

            public LoadEventData( string timelineId, string saveId )
            {
                this.timelineId = timelineId;
                this.saveId = saveId;
                this.objectActions = new List<Func<ILoader, IEnumerator>>();
                this.dataActions = new List<Func<ILoader, IEnumerator>>();
            }
        }

#warning TODO - use an event to add these strategies to the savers/loaders, they don't belong here.

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

        private static void CreateSaver( List<Func<ISaver, IEnumerator>> dataActions, List<Func<ISaver, IEnumerator>> objectActions )
        {
            _saver = new AsyncSaver( SerializationPauseFunc, SerializationUnpauseFunc, objectActions, dataActions );
        }

        private static void CreateLoader( List<Func<ILoader, IEnumerator>> dataActions, List<Func<ILoader, IEnumerator>> objectActions )
        {
            _loader = new AsyncLoader( SerializationPauseFunc, SerializationUnpauseFunc, objectActions, dataActions );
        }

        public static void EnsureDirectoryExists( string path )
        {
            if( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }
        }

        /// <summary>
        /// Asynchronously saves the current game state over multiple frames. <br/>
        /// The game should remain paused for the duration of the saving (this is generally handled automatically, but be careful).
        /// </summary>
        public static void BeginSaveAsync( string timelineId, string saveId, string saveName, string saveDescription )
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start saving a timeline while already saving or loading." );
            }

            EnsureDirectoryExists( SaveMetadata.GetRootDirectory( timelineId, saveId ) );

            SaveEventData e = new SaveEventData( timelineId, saveId );
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_SAVE, e );

            CreateSaver( e.objectActions, e.dataActions );

            _saver.SaveAsync( instance );

            CurrentTimeline.WriteToDisk();
            SaveMetadata savedSave = new SaveMetadata( timelineId, saveId );
            savedSave.Name = saveName;
            savedSave.Description = saveDescription;
            savedSave.FileVersion = SaveMetadata.CURRENT_SAVE_FILE_VERSION;
            savedSave.WriteToDisk();

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_SAVE, e );
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
                throw new InvalidOperationException( $"Can't start loading a timeline while already saving or loading." );
            }

            EnsureDirectoryExists( SaveMetadata.GetRootDirectory( timelineId, saveId ) );

            TimelineMetadata loadedTimeline = new TimelineMetadata( timelineId );
            loadedTimeline.ReadDataFromDisk();
            SaveMetadata loadedSave = new SaveMetadata( timelineId, saveId );
            loadedSave.ReadDataFromDisk();

            LoadEventData e = new LoadEventData( timelineId, saveId );
            CreateLoader( e.objectActions, e.dataActions );

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_LOAD, e );

            _loader.LoadAsync( instance );
            CurrentTimeline = loadedTimeline;

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_LOAD, e );
        }

        /// <summary>
        /// Creates a new default (empty) timeline and "loads" it.
        /// </summary>
        public static void CreateNew( string timelineId, string saveId, string timelineName, string timelineDescription )
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't create a new timeline while already saving or loading." );
            }

            TimelineMetadata newTimeline = new TimelineMetadata( timelineId );
            newTimeline.Name = timelineName;
            newTimeline.Description = timelineDescription;

            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_NEW );

            CurrentTimeline = newTimeline;
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_NEW );
        }
    }
}