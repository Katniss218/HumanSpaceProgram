using HSP.Content;
using HSP.Time;
using HSP.Timelines.Serialization;
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
    /// Manages the currently loaded timeline (i.e. a save or world). See <see cref="TimelineMetadata"/> and <see cref="SaveMetadata"/>.
    /// </summary>
    public class TimelineManager : SingletonMonoBehaviour<TimelineManager>
    {
        public struct SaveEventData
        {
            public readonly TimelineMetadata timeline;
            public readonly SaveMetadata save;

            public SaveEventData( TimelineMetadata timeline, SaveMetadata save )
            {
                this.timeline = timeline;
                this.save = save;
            }
        }

        public struct LoadEventData
        {
            public readonly TimelineMetadata timeline;
            public readonly SaveMetadata save;

            public LoadEventData( TimelineMetadata timeline, SaveMetadata save )
            {
                this.timeline = timeline;
                this.save = save;
            }
        }

        public struct StartNewEventData
        {
            public readonly ScenarioMetadata scenario;
            public readonly TimelineMetadata timeline;

            public StartNewEventData( ScenarioMetadata scenario, TimelineMetadata timeline )
            {
                this.scenario = scenario;
                this.timeline = timeline;
            }
        }

        /// <summary>
        /// Checks if a timeline is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading { get; private set; }

        /// <summary>
        /// Gets the scenario that the currently active timeline is based on.
        /// </summary>
        public static ScenarioMetadata CurrentScenario { get; private set; }

#warning TODO - use instance for storage, so it's removed when scene unloads.
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
        public static void BeginSaveAsync()
        {
            BeginSaveAsync( CurrentSave );
        }

        /// <summary>
        /// Asynchronously saves the current game state over multiple frames. <br/>
        /// The game should remain paused for the duration of the saving (this is generally handled automatically, but be careful).
        /// </summary>
        /// <param name="save">A new save instance that will be used to save, and also set as the active save.</param>
        public static void BeginSaveAsync( SaveMetadata save )
        {
            if( save == null )
            {
                throw new ArgumentNullException( nameof( save ), $"The save to save to must not be null. If you have intended to save as the persistent save, use `SaveMetadata.LoadPersistentFromDisk`." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start saving a timeline while already saving or loading." );
            }

            Directory.CreateDirectory( SaveMetadata.GetRootDirectory( save.TimelineID, save.SaveID ) );

            var eSave = new SaveEventData( CurrentTimeline, save );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_SAVE.ID, eSave );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE.ID, eSave );
            SaveLoadFinishFunc();

            CurrentTimeline.SaveToDisk();
            save.FileVersion = SaveMetadata.CURRENT_SAVE_FILE_VERSION;
            save.SaveToDisk();
            CurrentSave = save;
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_SAVE.ID, eSave );
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

            var eLoad = new LoadEventData( loadedTimeline, loadedSave );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_LOAD.ID, eLoad );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD.ID, eLoad );
            SaveLoadFinishFunc();
            CurrentTimeline = loadedTimeline;
            CurrentSave = loadedSave;
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_LOAD.ID, eLoad );
        }

        /// <summary>
        /// Creates a new default (empty) timeline and "loads" it.
        /// </summary>
        public static void BeginScenarioAsync( NamespacedID scenarioId, TimelineMetadata timeline )
        {
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't create a new timeline while already saving or loading." );
            }

            ScenarioMetadata loadedScenario = ScenarioMetadata.LoadFromDisk( scenarioId );
            var eNew = new StartNewEventData( loadedScenario, timeline );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_NEW.ID, eNew );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW.ID, eNew );
            SaveLoadFinishFunc();
            CurrentScenario = loadedScenario;
            CurrentTimeline = timeline;
            CurrentSave = new SaveMetadata( timeline.TimelineID );
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_NEW.ID, eNew );
        }
    }
}