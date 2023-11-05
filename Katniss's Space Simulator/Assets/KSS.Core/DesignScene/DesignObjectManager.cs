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
    public class DesignObjectManager : SingletonMonoBehaviour<DesignObjectManager>
    {
        static JsonSeparateFileSerializedDataHandler _designObjDataHandler = new JsonSeparateFileSerializedDataHandler();
        static JsonSingleExplicitHierarchyStrategy _designObjStrategy = new JsonSingleExplicitHierarchyStrategy( _designObjDataHandler, GetGameObject );

        [SerializeField]
        private DesignObject _designObj;

        [SerializeField]
        private List<Transform> _looseParts = new List<Transform>();

        /// <summary>
        /// Picks up the specified object (removes it from the actionable objects).
        /// </summary>
        public static void PickUp( Transform obj )
        {
            if( !IsActionable( obj ) )
            {
                throw new ArgumentException( $"object to pick up must be an actionable object.", nameof( obj ) );
            }

            if( IsRootOfDesignObj( obj ) )
            {
                instance._designObj.RootPart = null;
                obj.SetParent( null );
            }
            else
            {
                instance._looseParts.Remove( obj ); // sometimes will do nothing, since the part might not be a loose part.
                obj.SetParent( null );
            }
        }

        /// <summary>
        /// Places the selected object as the child of the specified object (adds to the actionable parts).
        /// </summary>
        /// <param name="parent">The new parent, can be null, in which case, the part will be placed as a loose part.</param>
        public static void Place( Transform obj, Transform parent )
        {
            if( IsActionable( obj ) )
            {
                throw new ArgumentException( $"object to place must NOT be an actionable object.", nameof( obj ) );
            }
            if( parent != null && !IsActionable( parent ) )
            {
                throw new ArgumentException( $"Parent must be null or an actionable object.", nameof( parent ) );
            }

            if( parent == null )
            {
                instance._looseParts.Add( obj );
            }
            obj.SetParent( parent );
        }

        /// <summary>
        /// Places the selected object as the new root of the design object.
        /// </summary>
        /// <remarks>
        /// This will destroy the already existing root part, if any.
        /// </remarks>
        public static void PlaceRoot( Transform obj )
        {
            if( IsActionable( obj ) )
            {
                throw new ArgumentException( $"object to place must NOT be an actionable object.", nameof( obj ) );
            }

            if( instance._designObj.RootPart != null )
            {
                Destroy( instance._designObj.RootPart );
            }
            obj.SetParent( instance._designObj.transform );
            instance._designObj.RootPart = obj;
        }

        /// <summary>
        /// True if the object can be interacted with (not part of the scenery, etc).
        /// </summary>
        public static bool IsActionable( Transform obj )
        {
            if( obj == null )
                return false;

            return instance._looseParts.Contains( obj.root )
                || obj.root == instance._designObj.transform;
        }

        /// <summary>
        /// Checks whether the specified object is part of the design object.
        /// </summary>
        public static bool IsAttachedToDesignObj( Transform obj )
        {
            if( obj == null )
                return false;

            return obj.root == instance._designObj.transform;
        }

        /// <summary>
        /// Checks whether the specified object is the root part of the design object.
        /// </summary>
        public static bool IsRootOfDesignObj( Transform obj )
        {
            if( obj == null )
                return false;

            return obj.parent == obj.root && obj.parent == instance._designObj.transform;
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

        public static void FinishSaveFunc()
        {
            TimeManager.LockTimescale = false;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_AFTER_SAVE, null );
        }
        
        public static void FinishLoadFunc()
        {
            TimeManager.LockTimescale = false;
            instance._designObj.RootPart = _designObjStrategy.LastSpawnedRoot.transform;
            if( !_wasPausedBeforeSerializing )
            {
                TimeManager.Unpause();
            }
            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_AFTER_LOAD, null );
        }

        private static void CreateSaver( IEnumerable<Func<ISaver, IEnumerator>> objectActions, IEnumerable<Func<ISaver, IEnumerator>> dataActions )
        {
            _saver = new AsyncSaver( StartFunc, FinishSaveFunc, objectActions, dataActions );
        }

        private static void CreateLoader( IEnumerable<Func<ILoader, IEnumerator>> objectActions, IEnumerable<Func<ILoader, IEnumerator>> dataActions )
        {
            _loader = new AsyncLoader( StartFunc, FinishLoadFunc, objectActions, dataActions );
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
#warning TODO - take the input from the user to create the metadata. I.e. the UI should automatically update the metadata in DesignObjectManager, since we don't know that Ui even exists here.
            Directory.CreateDirectory( CurrentVesselMetadata.GetRootDirectory() );
            _designObjDataHandler.ObjectsFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "objects.json" );
            _designObjDataHandler.DataFilename = Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "data.json" );

            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_BEFORE_SAVE, null );

            CreateSaver( new Func<ISaver, IEnumerator>[] { _designObjStrategy.SaveAsync_Object }, new Func<ISaver, IEnumerator>[] { _designObjStrategy.SaveAsync_Data } );

            _saver.SaveAsync( instance );

            CurrentVesselMetadata.WriteToDisk();
        }

        public static void LoadVessel( string vesselId )
        {
            VesselMetadata loadedVesselMetadata = new VesselMetadata( vesselId );
            loadedVesselMetadata.ReadDataFromDisk();

            // load current vessel from the files defined by metadata's ID.
            Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
            _designObjDataHandler.ObjectsFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "objects.json" );
            _designObjDataHandler.DataFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "data.json" );

            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_BEFORE_LOAD, null );
            CurrentVesselMetadata = loadedVesselMetadata; // CurrentVesselMetadata should be set after invoking before load.

            CreateLoader( new Func<ILoader, IEnumerator>[] { _designObjStrategy.LoadAsync_Object }, new Func<ILoader, IEnumerator>[] { _designObjStrategy.LoadAsync_Data } );
            _loader.LoadAsync( instance );
        }

        // ------

        private static GameObject GetGameObject()
        {
            return instance._designObj.RootPart.gameObject;
        }
    }
}