using KSS.Core.TimeWarp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.OverridableEvents;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Manages the currently loaded timeline (save/workspace). See <see cref="TimelineMetadata"/> and <see cref="SaveMetadata"/>.
    /// </summary>
    public static class TimelineManager
    {
        /// <summary>
        /// The saver used by the <see cref="TimelineManager"/> to serialize the currently loaded game state. <br/>
        /// Mod developers can hook into it to save additional data.
        /// </summary>
        public static AsyncSaver Saver { get; private set; }

        /// <summary>
        /// The loader used by the <see cref="TimelineManager"/> to deserialize a saved game state. <br/>
        /// Mod developers can hook into it to load additional data.
        /// </summary>
        public static AsyncLoader Loader { get; private set; }

        /// <summary>
        /// Contains information if a timeline is currently being either saved or loaded.
        /// </summary>
        public static bool IsSerializing { get; private set; } = false;

        static JsonPrefabAndDataStrategy _serializationStrat = new JsonPrefabAndDataStrategy();
        static TimelineMetadata _currentTimeline; // currently playing timeline.
        static bool _shouldUnpause = false;

        public static void SerializationPauseFunc()
        {
            TimeWarpManager.PreventPlayerChangingTimescale = true;
            _shouldUnpause = !TimeWarpManager.IsPaused;
            TimeWarpManager.Pause();
            IsSerializing = true;
        }

        public static void SerializationUnpauseFunc()
        {
            IsSerializing = false;
            if( _shouldUnpause )
            {
                TimeWarpManager.Unpause();
            }
            TimeWarpManager.PreventPlayerChangingTimescale = false;
        }

        static void CreateDefaultSaver()
        {
            Saver = new AsyncSaver(
                SerializationPauseFunc, SerializationUnpauseFunc,
                new Func<ISaver, IEnumerator>[] { _serializationStrat.SaveSceneObjects_Object },
                new Func<ISaver, IEnumerator>[] { _serializationStrat.SaveSceneObjects_Data }
            );
        }

        static void CreateDefaultLoader()
        {
            Loader = new AsyncLoader(
                SerializationPauseFunc, SerializationUnpauseFunc,
                new Func<ILoader, IEnumerator>[] { _serializationStrat.LoadSceneObjects_Object },
                new Func<ILoader, IEnumerator>[] { _serializationStrat.LoadSceneObjects_Data }
            );
        }

        public static void Save( string timelineId, string saveId = null ) // save the current game state.
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }

#warning TODO - check saver's state and prevent calling if not 'ready'.

            CreateDefaultSaver();
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_SAVER_CREATE );

            throw new NotImplementedException();
            // if universe is given, and saveId is null, overwrite the default (this shouldn't really be done in practice, but if you want to, you can).
            // if timeline is null, assume current.
        }

        public static void Load( string timelineId, string saveId = null ) // modifies the current game state.
        {
            if( string.IsNullOrEmpty( timelineId ) && string.IsNullOrEmpty( saveId ) )
            {
                throw new ArgumentException( $"Both can't be null." );
            }

#warning TODO - check loader's state and prevent calling if not 'ready'.

            CreateDefaultLoader();
            HSPEvent.EventManager.TryInvoke( HSPEvent.TIMELINE_LOADER_CREATE );

            throw new NotImplementedException();
            // if universe is given, and saveId is null, load the default.
            // if timeline is null, assume current.

            // additional loading/saving strategies can serialize things like in-game time, etc.
        }
    }
}