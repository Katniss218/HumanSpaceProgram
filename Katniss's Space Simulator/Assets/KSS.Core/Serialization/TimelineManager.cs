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
    public class TimelineManager : SingletonMonoBehaviour<TimelineManager>
    {
        public struct SaveEventData
        {
            public string timelineId;
            public string saveId;
            /// <summary>
            /// Use these to add save actions of the object stage.
            /// </summary>
            public List<Func<ISaver, IEnumerator>> objectActions;
            /// <summary>
            /// Use these to add save actions of the data stage.
            /// </summary>
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
            /// <summary>
            /// Use these to add load actions of the object stage.
            /// </summary>
            public List<Func<ILoader, IEnumerator>> objectActions;
            /// <summary>
            /// Use these to add load actions of the data stage.
            /// </summary>
            public List<Func<ILoader, IEnumerator>> dataActions;

            public LoadEventData( string timelineId, string saveId )
            {
                this.timelineId = timelineId;
                this.saveId = saveId;
                this.objectActions = new List<Func<ILoader, IEnumerator>>();
                this.dataActions = new List<Func<ILoader, IEnumerator>>();
            }
        }

        /// <summary>
        /// Checks if a timeline is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading =>
                (_saver != null && _saver.CurrentState != ISaver.State.Idle)
             || (_loader != null && _loader.CurrentState != ILoader.State.Idle);

        public static TimelineMetadata CurrentTimeline { get; private set; }

        private static AsyncSaver _saver;
        private static AsyncLoader _loader;

        private static bool _wasPausedBeforeSerializing = false;

        public static void SaveLoadStartFunc()
        {
            _wasPausedBeforeSerializing = TimeManager.IsPaused;
            TimeManager.Pause();
            TimeManager.LockTimescale = true;
        }

        public static void SaveFinishFunc()
        {
            TimeManager.LockTimescale = false;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_SAVE, _eSave ); // invoke here because otherwise the invoking method finishes before the coroutine.
        }
        
        public static void LoadFinishFunc()
        {
            TimeManager.LockTimescale = false;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_AFTER_LOAD, _eLoad ); // invoke here because otherwise the invoking method finishes before the coroutine.
        }

        private static void CreateSaver( IEnumerable<Func<ISaver, IEnumerator>> objectActions, IEnumerable<Func<ISaver, IEnumerator>> dataActions )
        {
            _saver = new AsyncSaver( SaveLoadStartFunc, SaveFinishFunc, objectActions, dataActions );
        }

        private static void CreateLoader( IEnumerable<Func<ILoader, IEnumerator>> objectActions, IEnumerable<Func<ILoader, IEnumerator>> dataActions )
        {
            _loader = new AsyncLoader( SaveLoadStartFunc, LoadFinishFunc, objectActions, dataActions );
        }

        private static SaveEventData _eSave;
        private static LoadEventData _eLoad;

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

            Directory.CreateDirectory( SaveMetadata.GetRootDirectory( timelineId, saveId ) );

            _eSave = new SaveEventData( timelineId, saveId );
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_SAVE, _eSave );
            CreateSaver( _eSave.objectActions, _eSave.dataActions );

            _saver.SaveAsync( instance );

            CurrentTimeline.WriteToDisk();
            SaveMetadata savedSave = new SaveMetadata( timelineId, saveId );
            savedSave.Name = saveName;
            savedSave.Description = saveDescription;
            savedSave.FileVersion = SaveMetadata.CURRENT_SAVE_FILE_VERSION;
            savedSave.WriteToDisk();
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

            Directory.CreateDirectory( SaveMetadata.GetRootDirectory( timelineId, saveId ) );

            TimelineMetadata loadedTimeline = new TimelineMetadata( timelineId );
            loadedTimeline.ReadDataFromDisk();
            SaveMetadata loadedSave = new SaveMetadata( timelineId, saveId );
            loadedSave.ReadDataFromDisk();

            _eLoad = new LoadEventData( timelineId, saveId );
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_BEFORE_LOAD, _eLoad );
            CreateLoader( _eLoad.objectActions, _eLoad.dataActions );

            _loader.LoadAsync( instance );
            CurrentTimeline = loadedTimeline;
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