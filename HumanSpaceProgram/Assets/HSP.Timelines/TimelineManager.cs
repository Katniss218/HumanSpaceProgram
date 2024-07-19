using HSP.Content.Timelines.Serialization;
using HSP.Time;
using System;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace HSP.Timelines
{
    /// <summary>
    /// Invoked before loading a new game state (timeline + save).
    /// </summary>
    public static class HSPEvent_BEFORE_TIMELINE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.load.before";
    }

    /// <summary>
    /// Invoked to load a new game state (timeline + save).
    /// </summary>
    public static class HSPEvent_ON_TIMELINE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.load";
    }

    /// <summary>
    /// Invoked after loading a new game state (timeline + save).
    /// </summary>
    public static class HSPEvent_AFTER_TIMELINE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.load.after";
    }

    /// <summary>
    /// Invoked before saving the current game state (timeline + save).
    /// </summary>
    public static class HSPEvent_BEFORE_TIMELINE_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.save.before";
    }

    /// <summary>
    /// Invoked to save the current game state (timeline + save).
    /// </summary>
    public static class HSPEvent_ON_TIMELINE_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.save";
    }

    /// <summary>
    /// Invoked after saving the current game state (timeline + save).
    /// </summary>
    public static class HSPEvent_AFTER_TIMELINE_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.save.after";
    }

    /// <summary>
    /// Invoked before creating a new game state (timeline + default save).
    /// </summary>
    public static class HSPEvent_BEFORE_TIMELINE_NEW
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.new.before";
    }

    /// <summary>
    /// Invoked after creating a new game state (timeline + default save).
    /// </summary>
    public static class HSPEvent_ON_TIMELINE_NEW
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.new";
    }

    /// <summary>
    /// Invoked to create a new game state (timeline + default save).
    /// </summary>
    public static class HSPEvent_AFTER_TIMELINE_NEW
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.new.after";
    }

    /// <summary>
    /// Manages the currently loaded timeline (a.k.a. save/workspace). See <see cref="TimelineMetadata"/> and <see cref="SaveMetadata"/>.
    /// </summary>
    public class TimelineManager : SingletonMonoBehaviour<TimelineManager>
    {
        public struct NewEventData
        {
            public string timelineId;
            public string saveId;

            public NewEventData( string timelineId, string saveId )
            {
                this.timelineId = timelineId;
                this.saveId = saveId;
            }
        }

        public struct SaveEventData
        {
            public string timelineId;
            public string saveId;

            public SaveEventData( string timelineId, string saveId )
            {
                this.timelineId = timelineId;
                this.saveId = saveId;
            }
        }

        public struct LoadEventData
        {
            public string timelineId;
            public string saveId;

            public LoadEventData( string timelineId, string saveId )
            {
                this.timelineId = timelineId;
                this.saveId = saveId;
            }
        }

        /// <summary>
        /// Checks if a timeline is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading { get; private set; }

        /// <summary>
        /// Gets the currently active timeline.
        /// </summary>
        public static TimelineMetadata CurrentTimeline { get; private set; }

        /// <summary>
        /// Gets the currently active save (if any).
        /// </summary>
        public static SaveMetadata CurrentSave { get; private set; }

        private static bool _wasPausedBeforeSerializing = false;

        public static BidirectionalReferenceStore RefStore { get; private set; }

        public static void SaveLoadStartFunc()
        {
            IsSavingOrLoading = true;
            _wasPausedBeforeSerializing = TimeManager.IsPaused;
            TimeManager.Pause();
            TimeManager.LockTimescale = true;
        }

        public static void SaveLoadFinishFunc()
        {
            TimeManager.LockTimescale = false;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
            IsSavingOrLoading = false;
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

            Directory.CreateDirectory( SaveMetadata.GetRootDirectory( timelineId, saveId ) );

            var eSave = new SaveEventData( timelineId, saveId );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_SAVE.ID, eSave );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE.ID, eSave );
            SaveLoadFinishFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_SAVE.ID, eSave );

            CurrentTimeline.SaveToDisk();
            SaveMetadata savedSave = new SaveMetadata( timelineId, saveId );
            savedSave.Name = saveName;
            savedSave.Description = saveDescription;
            savedSave.FileVersion = SaveMetadata.CURRENT_SAVE_FILE_VERSION;
            savedSave.SaveToDisk();
            CurrentSave = savedSave;
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

            TimelineMetadata loadedTimeline = TimelineMetadata.LoadFromDisk( timelineId );
            SaveMetadata loadedSave = SaveMetadata.LoadFromDisk( timelineId, saveId );

            var eLoad = new LoadEventData( timelineId, saveId );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_LOAD.ID, eLoad );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD.ID, eLoad );
            SaveLoadFinishFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_LOAD.ID, eLoad );

            CurrentTimeline = loadedTimeline;
            CurrentSave = loadedSave;
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

            var eNew = new NewEventData( timelineId, saveId );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_NEW.ID, eNew );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW.ID, eNew );
            SaveLoadFinishFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_NEW.ID, eNew );
            CurrentTimeline = newTimeline;
            CurrentSave = null;
        }
    }
}