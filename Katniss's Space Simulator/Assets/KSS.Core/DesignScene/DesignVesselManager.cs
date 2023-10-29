using KSS.Core.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;

namespace KSS.Core.DesignScene
{
    /// <summary>
    /// Manages the object (vessel/building/etc) being built in the design scene.
    /// </summary>
    public class DesignVesselManager : SingletonMonoBehaviour<DesignVesselManager>
    {
        static JsonExplicitHierarchyGameObjectsStrategy _vesselStrategy = new JsonExplicitHierarchyGameObjectsStrategy( GetGameObjects );

        private static IPartObject _vessel;

        /// <summary>
        /// Checks if a vessel/building/etc is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading =>
                (_saver != null && _saver.CurrentState != ISaver.State.Idle)
             || (_loader != null && _loader.CurrentState != ILoader.State.Idle);

        /// <summary>
        /// Modify this to point at a different craft file.
        /// </summary>
        public static VesselMetadata CurrentVesselMetadata { get; set; }

        private static AsyncSaver _saver;
        private static AsyncLoader _loader;

        private static bool _wasPausedBeforeSerializing = false;

        public static void StartFunc()
        {
            _wasPausedBeforeSerializing = TimeManager.IsPaused;
            TimeManager.Pause();
            TimeManager.LockTimescale = true;
        }

        public static void FinishFunc()
        {
            TimeManager.LockTimescale = false;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
        }

        private static void CreateSaver( IEnumerable<Func<ISaver, IEnumerator>> objectActions, IEnumerable<Func<ISaver, IEnumerator>> dataActions )
        {
            _saver = new AsyncSaver( StartFunc, FinishFunc, objectActions, dataActions );
        }

        private static void CreateLoader( IEnumerable<Func<ILoader, IEnumerator>> objectActions, IEnumerable<Func<ILoader, IEnumerator>> dataActions )
        {
            _loader = new AsyncLoader( StartFunc, FinishFunc, objectActions, dataActions );
        }

        // undos stored in files, preserved across sessions?

        public static void SaveVessel()
        {
            // save current vessel to the files defined by metadata's ID.
            TimelineManager.EnsureDirectoryExists( CurrentVesselMetadata.GetRootDirectory() );
            _vesselStrategy.ObjectsFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "objects.json" );
            _vesselStrategy.DataFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "data.json" );

            CreateSaver( new Func<ISaver, IEnumerator>[] { _vesselStrategy.Save_Object }, new Func<ISaver, IEnumerator>[] { _vesselStrategy.Save_Data } );

            _saver.SaveAsync( instance );

            CurrentVesselMetadata.WriteToDisk();
        }

        public static void LoadVessel( string vesselId )
        {
            VesselMetadata loadedVesselMetadata = new VesselMetadata( vesselId );
            loadedVesselMetadata.ReadDataFromDisk();

            // load current vessel from the files defined by metadata's ID.
            TimelineManager.EnsureDirectoryExists( loadedVesselMetadata.GetRootDirectory() );
            _vesselStrategy.ObjectsFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "objects.json" );
            _vesselStrategy.DataFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "data.json" );

            CreateLoader( new Func<ILoader, IEnumerator>[] { _vesselStrategy.Load_Object }, new Func<ILoader, IEnumerator>[] { _vesselStrategy.Load_Data } );
            _loader.LoadAsync( instance );
            CurrentVesselMetadata = loadedVesselMetadata;
        }

        // ------

        private static GameObject[] GetGameObjects()
        {
            return new GameObject[] { _vessel.gameObject };
        }
    }
}