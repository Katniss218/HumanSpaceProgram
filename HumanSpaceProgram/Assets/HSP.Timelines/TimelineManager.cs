using HSP.Time;
using HSP.Timelines.Serialization;
using System;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace HSP.Timelines
{
    public static class HSPEvent_BEFORE_SCENARIO_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save.before";
    }
    public static class HSPEvent_ON_SCENARIO_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save";
    }
    public static class HSPEvent_AFTER_SCENARIO_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save.after";
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
        public struct SaveScenarioEventData
        {
            public readonly ScenarioMetadata scenario;

            public SaveScenarioEventData( ScenarioMetadata scenario )
            {
                this.scenario = scenario;
            }
        }

        public struct SaveEventData
        {
            public readonly ScenarioMetadata scenario;
            public readonly TimelineMetadata timeline;
            public readonly SaveMetadata save;

            public SaveEventData( ScenarioMetadata scenario, TimelineMetadata timeline, SaveMetadata save )
            {
                this.scenario = scenario;
                this.timeline = timeline;
                this.save = save;
            }
        }

        public struct LoadEventData
        {
            public readonly ScenarioMetadata scenario;
            public readonly TimelineMetadata timeline;
            public readonly SaveMetadata save;

            public LoadEventData( ScenarioMetadata scenario, TimelineMetadata timeline, SaveMetadata save )
            {
                this.scenario = scenario;
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

        private ScenarioMetadata _currentScenario;
        /// <summary>
        /// Gets the scenario that the currently active timeline is based on.
        /// </summary>
        public static ScenarioMetadata CurrentScenario => instance._currentScenario;

        private TimelineMetadata _currentTimeline;
        /// <summary>
        /// Gets the currently active timeline.
        /// </summary>
        public static TimelineMetadata CurrentTimeline => instance._currentTimeline;

        private SaveMetadata _currentSave;
        /// <summary>
        /// Gets the currently active save (if any).
        /// </summary>
        public static SaveMetadata CurrentSave => instance._currentSave;


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
        /// Asynchronously saves the current game state to a scenario over multiple frames. <br/>
        /// The game should remain paused for the duration of the saving (this is generally handled automatically, but be careful).
        /// </summary>
        /// <param name="save">A new scenario instance that will be used to save.</param>
        public static void BeginScenarioSaveAsync( ScenarioMetadata scenario )
        {
            if( scenario == null )
            {
                throw new ArgumentNullException( nameof( scenario ), $"The scenario to save to must not be null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start saving a timeline while already saving or loading." );
            }

            Directory.CreateDirectory( scenario.GetRootDirectory() );

            var eScenario = new SaveScenarioEventData( scenario );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_SCENARIO_SAVE.ID, eScenario );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_SCENARIO_SAVE.ID, eScenario );
            SaveLoadFinishFunc();

            CurrentTimeline.SaveToDisk();
            scenario.SaveToDisk();
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_SCENARIO_SAVE.ID, eScenario );
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
                throw new ArgumentNullException( nameof( save ), $"The save to save to must not be null. If you intended to save as the persistent save, pass in the persistent save." );
            }
            if( CurrentScenario == null )
            {
                throw new InvalidOperationException( $"Can't begin saving, {nameof( CurrentScenario )} is null." );
            }
            if( CurrentTimeline == null )
            {
                throw new InvalidOperationException( $"Can't begin saving, {nameof( CurrentTimeline )} is null." );
            }
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't start saving a timeline while already saving or loading." );
            }

            Directory.CreateDirectory( save.GetRootDirectory() );

            var eSave = new SaveEventData( CurrentScenario, CurrentTimeline, save );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_SAVE.ID, eSave );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE.ID, eSave );
            SaveLoadFinishFunc();

            CurrentTimeline.SaveToDisk();
            save.FileVersion = SaveMetadata.CURRENT_SAVE_FILE_VERSION;
            save.SaveToDisk();
            instance._currentSave = save;
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

            ScenarioMetadata loadedScenario;
            TimelineMetadata loadedTimeline;
            SaveMetadata loadedSave;
            try
            {
                loadedTimeline = TimelineMetadata.LoadFromDisk( timelineId );
            }
            catch( Exception ex )
            {
                throw new TimelineNotFoundException( timelineId, ex );
            }

            try
            {   // Needs to be loaded after the timeline, obviously.
                loadedScenario = ScenarioMetadata.LoadFromDisk( loadedTimeline.ScenarioID );
            }
            catch( Exception ex )
            {
                throw new ScenarioNotFoundException( loadedTimeline.ScenarioID, ex );
            }

            try
            {
                loadedSave = SaveMetadata.LoadFromDisk( timelineId, saveId );
            }
            catch( Exception ex )
            {
                throw new SaveNotFoundException( timelineId, saveId, ex );
            }

            var eLoad = new LoadEventData( loadedScenario, loadedTimeline, loadedSave );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_LOAD.ID, eLoad );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD.ID, eLoad );
            SaveLoadFinishFunc();
            instance._currentScenario = loadedScenario;
            instance._currentTimeline = loadedTimeline;
            instance._currentSave = loadedSave;
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_LOAD.ID, eLoad );
        }

        /// <summary>
        /// Creates a new default (empty) timeline and "loads" it.
        /// </summary>
        public static void BeginNewTimelineAsync( TimelineMetadata timeline )
        {
            if( IsSavingOrLoading )
            {
                throw new InvalidOperationException( $"Can't create a new timeline while already saving or loading." );
            }

            ScenarioMetadata loadedScenario;
            try
            {
                loadedScenario = ScenarioMetadata.LoadFromDisk( timeline.ScenarioID );
            }
            catch( Exception ex )
            {
                throw new ScenarioNotFoundException( timeline.ScenarioID, ex );
            }

            var eNew = new StartNewEventData( loadedScenario, timeline );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_NEW.ID, eNew );
            SaveLoadStartFunc();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW.ID, eNew );
            SaveLoadFinishFunc();
            instance._currentScenario = loadedScenario;
            instance._currentTimeline = timeline;
            instance._currentSave = new SaveMetadata( timeline.TimelineID );
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_NEW.ID, eNew );
        }
    }
}