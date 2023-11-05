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
        static JsonSeparateFileSerializedDataHandler _vesselDataHandler = new JsonSeparateFileSerializedDataHandler();
        static JsonSingleExplicitHierarchyStrategy _vesselStrategy = new JsonSingleExplicitHierarchyStrategy( _vesselDataHandler, GetGameObject );

        private DesignObject _designObj;

        private List<Transform> _looseParts = new List<Transform>();

        public static void PickUp( Transform t )
        {
            if( IsRootOfDesignObj( t ) )
            {
                instance._designObj.RootPart = null;
                t.SetParent( null );
            }
            else
            {
                instance._looseParts.Remove( t );
                //t.SetParent( null ); not needed
            }
        }

        public static void Place( Transform t, Transform parent )
        {
            if( parent == null )
                instance._looseParts.Add( t );
            t.SetParent( parent );
        }
        
        /// <summary>
        /// Places the selected object as the new root of the design object.
        /// </summary>
        public static void PlaceRoot( Transform t )
        {
            t.SetParent( instance._designObj.transform );
            instance._designObj.RootPart = t;
        }

        /// <summary>
        /// True if the object can be manipulated in the design scene (not part of the scenery, etc).
        /// </summary>
        public static bool IsActionable( Transform t )
        {
            return instance._looseParts.Contains( t.root )
                || t.root == instance._designObj.transform;
        }

        public static bool IsAttachedToDesignObj( Transform t )
        {
            return t.root == instance._designObj.transform;
        }

        public static bool IsRootOfDesignObj( Transform t )
        {
            return t.parent == t.root && t.parent == instance._designObj.transform;
        }

        public static bool DesignObjectHasRootPart()
        {
            return instance._designObj.RootPart != null;
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
            instance._designObj.RootPart = _vesselStrategy.LastSpawnedRoot.transform;
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

        /*
        When the ship is loaded, the entire thing is serialized, then each action additionally operates on that serialized data. Creating patches adding or removing only what has changed.
        then when the time to undo/redo comes, the changes are applied to the existing vessel.
        - for that, we need a strategy that can remove or add objects, and apply data to existing objects.
          - for that we need to keep the object's IDs in the loader.
          - it would work like this:
            1. Have the IDs of existing objects in the strategy.
            2. Have a stack of patches that is updated every time an action happens. These patches contain the serialized data. This can be stored separately. Our strat is a `ExplicitHierarchyPatchStrategy`
            3. Add the IDs to the saver/loader immediately.
            4. Apply the selected patch:
               1. O: Delete() the objects that need to be removed (if any).
               2. O: Create the objects that need to be added (if any).
               3. D: Apply data to the objects (if any).
            5. Get the IDs to persist for later.
        */

        void Awake()
        {
            _designObj = DesignObjectFactory.CreatePartless( Vector3.zero, Quaternion.identity );
        }



        public static void SaveVessel()
        {
            // save current vessel to the files defined by metadata's ID.
            Directory.CreateDirectory( CurrentVesselMetadata.GetRootDirectory() );
            _vesselDataHandler.ObjectsFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "objects.json" );
            _vesselDataHandler.DataFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "data.json" );

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
            _vesselDataHandler.ObjectsFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "objects.json" );
            _vesselDataHandler.DataFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "data.json" );

            CreateLoader( new Func<ILoader, IEnumerator>[] { _vesselStrategy.LoadAsync_Object }, new Func<ILoader, IEnumerator>[] { _vesselStrategy.LoadAsync_Data } );
            _loader.LoadAsync( instance );
            CurrentVesselMetadata = loadedVesselMetadata;
        }

        // ------

        private static GameObject GetGameObject()
        {
            return instance._designObj.RootPart.gameObject;
        }
    }
}