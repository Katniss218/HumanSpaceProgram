using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.OverridableEvents;
using UnityPlus.Serialization;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Manages the currently loaded timeline (save/workspace). See <see cref="TimelineMetadata"/> and <see cref="SaveMetadata"/>.
    /// </summary>
    public static class TimelineManager
    {
        static TimelineMetadata _currentTimeline; // currently playing timeline.

        // Public saver/loader for mod compat - mods might want to modify what is saved and how.
        public static AsyncSaver Saver;
        public static AsyncLoader Loader;

        static UnityPlus.Serialization.Strategies.JsonPrefabAndDataStrategy _serializationStrat = new UnityPlus.Serialization.Strategies.JsonPrefabAndDataStrategy();

        public static void SerializationPauseFunc()
        {
            TimeWarp.TimeWarpManager.PreventPlayerChangingTimescale = true;
            TimeWarp.TimeWarpManager.Pause();
        }

        public static void SerializationUnpauseFunc()
        {
            TimeWarp.TimeWarpManager.Unpause();
            TimeWarp.TimeWarpManager.PreventPlayerChangingTimescale = false;
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
            HSPOverridableEvent.EventManager.TryInvoke( HSPOverridableEvent.TIMELINE_SAVER_CREATE );

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
            HSPOverridableEvent.EventManager.TryInvoke( HSPOverridableEvent.TIMELINE_LOADER_CREATE );

            throw new NotImplementedException();
            // if universe is given, and saveId is null, load the default.
            // if timeline is null, assume current.

            // additional loading/saving strategies can serialize things like in-game time, etc.
        }
    }
}