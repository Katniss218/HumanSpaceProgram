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
        static JsonSingleExplicitHierarchyStrategy _vesselStrategy = new JsonSingleExplicitHierarchyStrategy( GetGameObject );

        private IPartObject _vessel;

        public static bool VesselExists => instance._vessel != null;

        private List<Transform> _looseParts = new List<Transform>();

#warning TODO - these methods are quite ugly.
        public static bool CanPickup( Transform t )
        {
            return instance._looseParts.Contains( t.root ) || t.root == instance._vessel.transform;
        }

        public static void VesselPlace( Transform t )
        {
            instance._vessel = t.GetComponent<IPartObject>();
        }

        public static void GhostPlace( Transform t )
        {
            instance._looseParts.Add( t );
        }
        
        public static void VesselPickup()
        {
            instance._vessel = null;
        }
        
        public static void GhostPickup( Transform t )
        {
            instance._looseParts.Remove( t );
        }

        public static bool IsVessel( Transform t )
        {
            if( instance._vessel == null )
                return false;
            return t.root == instance._vessel.transform;
        }

        public static void TryCreateNewVessel( GameObject root )
        {
            if( instance._vessel == null )
            {
                //Vessel v = new VesselFactory().Create( Vector3Dbl.zero, QuaternionDbl.identity, root );
                //instance._vessel = v;
            }
        }

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
            instance._vessel = _vesselStrategy.LastSpawnedRoot.GetComponent<IPartObject>();
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
            Directory.CreateDirectory( CurrentVesselMetadata.GetRootDirectory() );
            _vesselStrategy.ObjectsFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "objects.json" );
            _vesselStrategy.DataFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "data.json" );

            CreateSaver( new Func<ISaver, IEnumerator>[] { _vesselStrategy.SaveAsync_Object }, new Func<ISaver, IEnumerator>[] { _vesselStrategy.SaveAsync_Data } );

            _saver.SaveAsync( instance );

            CurrentVesselMetadata.WriteToDisk();
        }

        public static void LoadVessel( string vesselId )
        {
            VesselMetadata loadedVesselMetadata = new VesselMetadata( vesselId );
            loadedVesselMetadata.ReadDataFromDisk();

            // load current vessel from the files defined by metadata's ID.
            Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
            _vesselStrategy.ObjectsFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "objects.json" );
            _vesselStrategy.DataFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "data.json" );

            CreateLoader( new Func<ILoader, IEnumerator>[] { _vesselStrategy.LoadAsync_Object }, new Func<ILoader, IEnumerator>[] { _vesselStrategy.LoadAsync_Data } );
            _loader.LoadAsync( instance );
            CurrentVesselMetadata = loadedVesselMetadata;
        }

        // ------

        private static GameObject GetGameObject()
        {
            return instance._vessel.gameObject;
        }
    }
}