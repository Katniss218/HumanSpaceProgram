using HSP.Content;
using HSP.Content.Migrations;
using HSP.Content.Mods;
using HSP.Time;
using HSP.Timelines.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;
using Version = HSP.Content.Version;

namespace HSP.Timelines
{
    /// <summary>
    /// Invoked before saving the current scene as a scenario.
    /// </summary>
    public static class HSPEvent_BEFORE_SCENARIO_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save.before";
    }
    /// <summary>
    /// Invoked to save the current scene as a scenario.
    /// </summary>
    public static class HSPEvent_ON_SCENARIO_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save";
    }
    /// <summary>
    /// Invoked after saving the current scene as a scenario.
    /// </summary>
    public static class HSPEvent_AFTER_SCENARIO_SAVE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save.after";
    }
    public static class HSPEvent_ON_SCENARIO_SAVE_ERROR
    {
        public sealed class EventData
        {
            public readonly ScenarioSaveEventData data;

            public EventData( ScenarioSaveEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save.error";
    }
    public static class HSPEvent_ON_SCENARIO_SAVE_SUCCESS
    {
        public sealed class EventData
        {
            public readonly ScenarioSaveEventData data;

            public EventData( ScenarioSaveEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".scenario.save.successful";
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
    public static class HSPEvent_ON_TIMELINE_SAVE_ERROR
    {
        public sealed class EventData
        {
            public readonly TimelineSaveEventData data;

            public EventData( TimelineSaveEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.save.error";
    }
    public static class HSPEvent_ON_TIMELINE_SAVE_SUCCESS
    {
        public sealed class EventData
        {
            public readonly TimelineSaveEventData data;

            public EventData( TimelineSaveEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.save.successful";
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
    public static class HSPEvent_ON_TIMELINE_LOAD_ERROR
    {
        public sealed class EventData
        {
            public readonly TimelineLoadEventData data;

            public EventData( TimelineLoadEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.load.error";
    }
    public static class HSPEvent_ON_TIMELINE_LOAD_SUCCESS
    {
        public sealed class EventData
        {
            public readonly TimelineLoadEventData data;

            public EventData( TimelineLoadEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.load.successful";
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
    public static class HSPEvent_ON_TIMELINE_NEW_ERROR
    {
        public sealed class EventData
        {
            public readonly TimelineNewEventData data;

            public EventData( TimelineNewEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.new.error";
    }
    public static class HSPEvent_ON_TIMELINE_NEW_SUCCESS
    {
        public sealed class EventData
        {
            public readonly TimelineNewEventData data;

            public EventData( TimelineNewEventData data )
            {
                this.data = data;
            }
        }
        public const string ID = HSPEvent.NAMESPACE_HSP + ".timeline.new.successful";
    }

    //
    //
    //

    /// <summary>
    /// Manages the currently loaded timeline (i.e. a save or world). See <see cref="TimelineMetadata"/> and <see cref="SaveMetadata"/>.
    /// </summary>
    public class TimelineManager : SingletonMonoBehaviour<TimelineManager>
    {
        /// <summary>
        /// Checks if a timeline is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading { get; private set; }

        private ScenarioMetadata _currentScenario;
        /// <summary>
        /// Gets the scenario that the currently active timeline is based on.
        /// </summary>
        public static ScenarioMetadata CurrentScenario => instanceExists ? instance._currentScenario : null;

        private TimelineMetadata _currentTimeline;
        /// <summary>
        /// Gets the currently active timeline.
        /// </summary>
        public static TimelineMetadata CurrentTimeline => instanceExists ? instance._currentTimeline : null;

        private SaveMetadata _currentSave;
        /// <summary>
        /// Gets the currently active save (if any).
        /// </summary>
        public static SaveMetadata CurrentSave => instanceExists ? instance._currentSave : null;


        private static bool _wasPausedBeforeSerializing = false;
        public static BidirectionalReferenceStore RefStore { get; private set; }

        public static void SaveLoadStartLockPause()
        {
            IsSavingOrLoading = true;
            _wasPausedBeforeSerializing = TimeManager.IsPaused;
            TimeManager.Pause();
            TimeManager.LockTimescale = true;
        }

        public static void SaveLoadFinishUnlockUnpause()
        {
            TimeManager.LockTimescale = false;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
            IsSavingOrLoading = false;
        }

        //
        //
        //

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

            string rootDirectory = scenario.GetRootDirectory();
            if( Directory.Exists( rootDirectory ) )
                Directory.Delete( rootDirectory, true ); // Delete the old directory (if exists) to stop old, not-overwritten data remaining there.
            else
                Directory.CreateDirectory( rootDirectory );

            ScenarioSaveEventData eventData = new ScenarioSaveEventData( scenario );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_SCENARIO_SAVE.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting scenario save due to errors in {nameof( HSPEvent_BEFORE_SCENARIO_SAVE )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_SCENARIO_SAVE_ERROR.ID, new HSPEvent_ON_SCENARIO_SAVE_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadStartLockPause();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_SCENARIO_SAVE.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting scenario save due to errors in {nameof( HSPEvent_ON_SCENARIO_SAVE )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_SCENARIO_SAVE_ERROR.ID, new HSPEvent_ON_SCENARIO_SAVE_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadFinishUnlockUnpause();

            scenario.FileVersion = ScenarioMetadata.CURRENT_SCENARIO_FILE_VERSION;
            scenario.ModVersions = HumanSpaceProgramModLoader.GetCurrentSaveModVersions();
            scenario.SaveToDisk();
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_SCENARIO_SAVE.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting scenario save due to errors in {nameof( HSPEvent_AFTER_SCENARIO_SAVE )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_SCENARIO_SAVE_ERROR.ID, new HSPEvent_ON_SCENARIO_SAVE_ERROR.EventData( eventData ) );
                return;
            }

            // Important that this runs after, as a separate event. Otherwise we might show the UI success dialog before there is an error in the next listener.
            // Also saves on needless sorting of listeners.
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_SCENARIO_SAVE_SUCCESS.ID, new HSPEvent_ON_SCENARIO_SAVE_SUCCESS.EventData( eventData ) );
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

            string rootDirectory = save.GetRootDirectory();
            if( Directory.Exists( rootDirectory ) )
                Directory.Delete( rootDirectory, true ); // Delete the old directory (if exists) to stop old, not-overwritten data remaining there.
            else
                Directory.CreateDirectory( rootDirectory );

            TimelineSaveEventData eventData = new TimelineSaveEventData( CurrentScenario, CurrentTimeline, save );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_SAVE.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting timeline save due to errors in {nameof( HSPEvent_BEFORE_TIMELINE_SAVE )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE_ERROR.ID, new HSPEvent_ON_TIMELINE_SAVE_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadStartLockPause();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting timeline save due to errors in {nameof( HSPEvent_ON_TIMELINE_SAVE )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE_ERROR.ID, new HSPEvent_ON_TIMELINE_SAVE_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadFinishUnlockUnpause();

            CurrentTimeline.SaveToDisk();
            save.FileVersion = SaveMetadata.CURRENT_SAVE_FILE_VERSION;
            save.ModVersions = HumanSpaceProgramModLoader.GetCurrentSaveModVersions();
            save.SaveToDisk();
            instance._currentSave = save;
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_SAVE.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting timeline save due to errors in {nameof( HSPEvent_AFTER_TIMELINE_SAVE )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE_ERROR.ID, new HSPEvent_ON_TIMELINE_SAVE_ERROR.EventData( eventData ) );
                return;
            }

            // Important that this runs after, as a separate event. Otherwise we might show the UI success dialog before there is an error in the next listener.
            // Also saves on needless sorting of listeners.
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_SAVE_SUCCESS.ID, new HSPEvent_ON_TIMELINE_SAVE_SUCCESS.EventData( eventData ) );
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
            if( !HumanSpaceProgramModLoader.AreCompatibleModsLoaded( loadedScenario.ModVersions ) )
            {
                throw new IncompatibleSaveException();
            }

            try
            {
                loadedSave = SaveMetadata.LoadFromDisk( timelineId, saveId );
            }
            catch( Exception ex )
            {
                throw new SaveNotFoundException( timelineId, saveId, ex );
            }
            if( !HumanSpaceProgramModLoader.AreCompatibleModsLoaded( loadedSave.ModVersions ) )
            {
                throw new IncompatibleSaveException();
            }

#warning TODO - check if newer version
            if( NeedsMigration( loadedSave ) )
            {
                throw new IncompatibleSaveException( $"Tried to load an unmigrated save." );
            }

            TimelineLoadEventData eventData = new TimelineLoadEventData( loadedScenario, loadedTimeline, loadedSave );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_LOAD.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting timeline load due to errors in {nameof( HSPEvent_BEFORE_TIMELINE_LOAD )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD_ERROR.ID, new HSPEvent_ON_TIMELINE_LOAD_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadStartLockPause();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting timeline load due to errors in {nameof( HSPEvent_ON_TIMELINE_LOAD )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD_ERROR.ID, new HSPEvent_ON_TIMELINE_LOAD_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadFinishUnlockUnpause();

            instance._currentScenario = loadedScenario;
            instance._currentTimeline = loadedTimeline;
            instance._currentSave = loadedSave;
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_LOAD.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting timeline load due to errors in {nameof( HSPEvent_AFTER_TIMELINE_LOAD )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD_ERROR.ID, new HSPEvent_ON_TIMELINE_LOAD_ERROR.EventData( eventData ) );
                return;
            }

            // Important that this runs after, as a separate event. Otherwise we might show the UI success dialog before there is an error in the next listener.
            // Also saves on needless sorting of listeners.
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_LOAD_SUCCESS.ID, new HSPEvent_ON_TIMELINE_LOAD_SUCCESS.EventData( eventData ) );
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
            if( !HumanSpaceProgramModLoader.AreCompatibleModsLoaded( loadedScenario.ModVersions ) )
            {
                throw new IncompatibleSaveException();
            }

            TimelineNewEventData eventData = new TimelineNewEventData( loadedScenario, timeline );
            RefStore = new BidirectionalReferenceStore();

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_TIMELINE_NEW.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting new timeline creation due to errors in {nameof( HSPEvent_BEFORE_TIMELINE_NEW )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW_ERROR.ID, new HSPEvent_ON_TIMELINE_NEW_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadStartLockPause();
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting new timeline creation due to errors in {nameof( HSPEvent_ON_TIMELINE_NEW )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW_ERROR.ID, new HSPEvent_ON_TIMELINE_NEW_ERROR.EventData( eventData ) );
                return;
            }
            SaveLoadFinishUnlockUnpause();

            instance._currentScenario = loadedScenario;
            instance._currentTimeline = timeline;
            instance._currentSave = new SaveMetadata( timeline.TimelineID );
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_TIMELINE_NEW.ID, eventData );
            if( eventData.HasErrors )
            {
                Debug.LogError( $"Aborting new timeline creation due to errors in {nameof( HSPEvent_AFTER_TIMELINE_NEW )} phase." );
                HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW_ERROR.ID, new HSPEvent_ON_TIMELINE_NEW_ERROR.EventData( eventData ) );
                return;
            }

            // Important that this runs after, as a separate event. Otherwise we might show the UI success dialog before there is an error in the next listener.
            // Also saves on needless sorting of listeners.
            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_TIMELINE_NEW_SUCCESS.ID, new HSPEvent_ON_TIMELINE_NEW_SUCCESS.EventData( eventData ) );
        }

        //
        //
        //

        public static void BackupScenario( ScenarioMetadata scenario )
        {
            if( scenario == null )
                throw new ArgumentNullException( nameof( scenario ) );

            BackupUtil.BackupDirectory( scenario.GetRootDirectory(), Path.GetDirectoryName( scenario.GetRootDirectory() ) );
        }

        public static void BackupSave( SaveMetadata save )
        {
            if( save == null )
                throw new ArgumentNullException( nameof( save ) );

            BackupUtil.BackupDirectory( save.GetRootDirectory(), Path.GetDirectoryName( save.GetRootDirectory() ) );
        }

        public static void RestoreBackup( ScenarioMetadata scenario )
        {
            if( scenario == null )
                throw new ArgumentNullException( nameof( scenario ) );

            BackupUtil.RestoreBackup( BackupUtil.GetLatestBackupFile( scenario.GetRootDirectory(), Path.GetDirectoryName( scenario.GetRootDirectory() ) ), scenario.GetRootDirectory() );
        }

        public static void RestoreBackup( SaveMetadata save )
        {
            if( save == null )
                throw new ArgumentNullException( nameof( save ) );

            BackupUtil.RestoreBackup( BackupUtil.GetLatestBackupFile( save.GetRootDirectory(), Path.GetDirectoryName( save.GetRootDirectory() ) ), save.GetRootDirectory() );
        }


        public static bool NeedsMigration( SaveMetadata save )
        {
            return NeedsMigration( save.ModVersions );
        }

        public static bool NeedsMigration( ScenarioMetadata scenario )
        {
            return NeedsMigration( scenario.ModVersions );
        }

        private static bool NeedsMigration( Dictionary<string, Version> modVersions )
        {
            if( modVersions == null || modVersions.Count == 0 )
            {
                return false;
            }

            var loadedModVersions = HumanSpaceProgramModLoader.GetCurrentModVersions();

            foreach( var modVersion in loadedModVersions )
            {
                if( !modVersions.TryGetValue( modVersion.Key, out var savedVersion ) )
                {
                    // Mod is new, no migration needed.
                    continue;
                }

                if( !Version.AreCompatible( modVersion.Value, savedVersion ) )
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Applies migrations to a save file.
        /// </summary>
        /// <param name="save">The save metadata to migrate</param>
        public static void MigrateSave( SaveMetadata save, bool force = false )
        {
            MigrationRegistry.Migrate( save.GetRootDirectory(), save.ModVersions, HumanSpaceProgramModLoader.GetCurrentModVersions(), force );
            save.ModVersions = GetCurrentMatching( save.ModVersions );
            save.SaveToDisk();
        }

        /// <summary>
        /// Applies migrations to a save file.
        /// </summary>
        /// <param name="scenario">The scenario to migrate</param>
        public static void MigrateScenario( ScenarioMetadata scenario, bool force = false )
        {
            MigrationRegistry.Migrate( scenario.GetRootDirectory(), scenario.ModVersions, HumanSpaceProgramModLoader.GetCurrentModVersions(), force );
            scenario.ModVersions = GetCurrentMatching( scenario.ModVersions );
            scenario.SaveToDisk();
        }

        public static Dictionary<string, Version> GetCurrentMatching( Dictionary<string, Version> modVersions )
        {
            Dictionary<string, Version> newDict = new Dictionary<string, Version>();
            foreach( var modId in modVersions.Keys )
            {
                newDict.Add( modId, HumanSpaceProgramModLoader.GetLoadedMod( modId ).Version );
            }
            return newDict;
        }
    }
}