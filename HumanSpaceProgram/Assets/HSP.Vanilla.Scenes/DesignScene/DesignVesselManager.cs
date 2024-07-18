using HSP.Time;
using HSP.Vessels;
using HSP.Vessels.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Scenes.DesignScene
{
    /// <summary>
    /// Manages the object (vessel/building/etc) being built in the design scene.
    /// </summary>
    public class DesignVesselManager : SingletonMonoBehaviour<DesignVesselManager>
    {
        private Vessel _designObj;
        /// <summary>
        /// Returns the object currently being edited.
        /// </summary>
        public static Vessel DesignObject => instance._designObj;

        /// <summary>
        /// Parts that are loosely dropped in the design scene, ghosted out.
        /// </summary>
        private List<Transform> _looseParts = new List<Transform>();

        /// <summary>
        /// True if the object can be interacted with (picked up, moved, rotated, etc).
        /// </summary>
        public static bool IsLooseOrPartOfDesignObject( Transform obj )
        {
            if( obj == null )
                return false;

            return instance._looseParts.Contains( obj.root ) || obj.IsChildOf( instance._designObj.transform );
        }

        /// <summary>
        /// Tries to pick up the specified object (unparents it, and removes from actionable objects).
        /// </summary>
        public static bool TryDetach( Transform obj )
        {
            if( !IsLooseOrPartOfDesignObject( obj ) )
            {
                return false;
            }

            if( obj == instance._designObj.RootPart )
            {
                instance._designObj.RootPart = null;
                return true;
            }

            instance._looseParts.Remove( obj ); // sometimes will do nothing, since the part might not be a loose part.
            obj.SetParent( null );
            return true;
        }

        /// <summary>
        /// Returns the root of every object that can have an object parented to it.
        /// </summary>
        public static IEnumerable<Transform> GetAttachableRoots()
        {
            return instance._designObj.RootPart == null
                ? new Transform[] { }
                : new Transform[] { instance._designObj.RootPart };
        }

        /// <summary>
        /// Checks whether an object can be parented to the specified object.
        /// </summary>
        public static bool CanHaveChildren( Transform parent )
        {
            if( parent == null )
                return true;

            return parent.root == instance._designObj.transform;
        }

        /// <summary>
        /// Places the selected object as the child of the specified object (adds to the actionable parts).
        /// </summary>
        /// <param name="parent">The new parent, can be null, in which case, the part will be placed as a loose part.</param>
        public static bool TryAttach( Transform obj, Transform parent )
        {
            if( IsLooseOrPartOfDesignObject( obj ) )
            {
                return false;
            }
            if( !CanHaveChildren( parent ) )
            {
                return false;
            }

            // Place as loose or as root of vessel.
            if( parent == null )
            {
                if( instance._designObj.RootPart == null )
                {
                    instance._designObj.transform.SetPositionAndRotation( obj.position, obj.rotation );
                    instance._designObj.RootPart = obj;
                    return true;
                }
                instance._looseParts.Add( obj );
            }

            obj.SetParent( parent );
            return true;
        }

        /// <summary>
        /// Places the selected object as the new root of the design object.
        /// </summary>
        /// <remarks>
        /// This will destroy the already existing root part, if any.
        /// </remarks>
        public static bool TryAttachRoot( Transform obj )
        {
            if( IsLooseOrPartOfDesignObject( obj ) )
            {
                return false;
            }

            if( DesignObject.RootPart != null )
            {
                Destroy( DesignObject.RootPart );
            }
            DesignObject.RootPart = obj;
            return true;
        }

        /// <summary>
        /// Checks if a vessel/building/etc is currently being either saved or loaded.
        /// </summary>
        public static bool IsSavingOrLoading { get; private set; }

        /// <summary>
        /// Specifies which craft file to save the vessel to.
        /// </summary>
        public static VesselMetadata CurrentVesselMetadata { get; set; }

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
            GameObject gameObject = new GameObject( $"DesignObject" );

            _designObj = gameObject.AddComponent<Vessel>();
            _designObj.transform.SetPositionAndRotation( Vector3.zero, Quaternion.identity );
        }



        public static void SaveVessel()
        {
            // save current vessel to the files defined by metadata's ID.
            Directory.CreateDirectory( CurrentVesselMetadata.GetRootDirectory() );
            JsonSerializedDataHandler _designObjDataHandler = new JsonSerializedDataHandler( Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "gameobjects.json" ) );

            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_BEFORE_SAVE, null );

            var data = SerializationUnit.Serialize( GetGameObject() );

            CurrentVesselMetadata.SaveToDisk();
            _designObjDataHandler.Write( data );
            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_AFTER_SAVE, null );
        }

        public static void LoadVessel( string vesselId )
        {
            VesselMetadata loadedVesselMetadata = VesselMetadata.LoadFromDisk( vesselId );

            // load current vessel from the files defined by metadata's ID.
            Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
            JsonSerializedDataHandler _designObjDataHandler = new JsonSerializedDataHandler( Path.Combine( loadedVesselMetadata.GetRootDirectory(), "gameobjects.json" ) );

            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_BEFORE_LOAD, null );
            CurrentVesselMetadata = loadedVesselMetadata; // CurrentVesselMetadata should be set after invoking before load.

            GameObject go = SerializationUnit.Deserialize<GameObject>( _designObjDataHandler.Read() );

            DesignObject.RootPart = go.transform;
            HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_AFTER_LOAD, null );
        }

        // ------

        private static GameObject GetGameObject()
        {
            if( DesignObject.RootPart == null )
                throw new InvalidOperationException( $"Can't save, the design object is empty." );

            return DesignObject.RootPart.gameObject;
        }
    }
}