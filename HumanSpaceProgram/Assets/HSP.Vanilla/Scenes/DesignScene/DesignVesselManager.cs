using HSP.Content.Vessels.Serialization;
using HSP.Time;
using HSP.Vessels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Formats;

namespace HSP.Vanilla.Scenes.DesignScene
{
    /// <summary>
    /// Invoked before the vessel is loaded in the design scene.
    /// </summary>
    public static class HSPEvent_BEFORE_DESIGN_SCENE_VESSEL_LOADED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.load.before";
    }

    /// <summary>
    /// Invoked after the vessel is loaded in the design scene.
    /// </summary>
    public static class HSPEvent_AFTER_DESIGN_SCENE_VESSEL_LOADED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.load.after";
    }

    /// <summary>
    /// Invoked before the vessel is saved in the design scene.
    /// </summary>
    public static class HSPEvent_BEFORE_DESIGN_SCENE_VESSEL_SAVED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.save.before";
    }

    /// <summary>
    /// Invoked after the vessel is saved in the design scene.
    /// </summary>
    public static class HSPEvent_AFTER_DESIGN_SCENE_VESSEL_SAVED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".designscene.save.after";
    }

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

        public static void SetDesignObject( Vessel vessel )
        {
            instance._designObj = vessel;
            ActiveVesselManager.ActiveObject = vessel?.transform;
        }

        public static void ClearDesignObject()
        {
            instance._designObj = null;
            ActiveVesselManager.ActiveObject = null;
        }

        /// <summary>
        /// Parts that are loosely dropped in the design scene, ghosted out.
        /// </summary>
        private List<Vessel> _looseParts = new List<Vessel>();

        public static void AddLoosePart( Vessel vessel )
        {
            if( vessel != null && !instance._looseParts.Contains( vessel ) )
            {
                instance._looseParts.Add( vessel );
            }
        }

        public static void RemoveLoosePart( Vessel vessel )
        {
            if( vessel != null )
            {
                instance._looseParts.Remove( vessel );
            }
        }

        /// <summary>
        /// True if the object can be interacted with (picked up, moved, rotated, etc).
        /// </summary>
        public static bool TryGetPart( Transform obj, out VesselPart part )
        {
            part = VesselPart.GetPart( obj );
            if( part == null )
                return false;

            if( DesignObject != null && part.Vessel == DesignObject )
                return true;
            if( instance._looseParts.Contains( part.Vessel ) )
                return true;

            return false;
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


        public static void SaveVessel()
        {
            // save current vessel to the files defined by metadata's ID.
            Directory.CreateDirectory( CurrentVesselMetadata.GetRootDirectory() );
            FileSerializedDataHandler _designObjDataHandler = new FileSerializedDataHandler( Path.Combine( CurrentVesselMetadata.GetRootDirectory(), "gameobjects.json" ), JsonFormat.Instance );

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_DESIGN_SCENE_VESSEL_SAVED.ID, null );

            var data = SerializationUnit.Serialize( GetGameObject() );

            CurrentVesselMetadata.SaveToDisk();
            _designObjDataHandler.Write( data );
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_DESIGN_SCENE_VESSEL_SAVED.ID, null );
        }

        public static void LoadVessel( string vesselId )
        {
            VesselMetadata loadedVesselMetadata = VesselMetadata.LoadFromDisk( vesselId );

            // load current vessel from the files defined by metadata's ID.
            Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
            FileSerializedDataHandler _designObjDataHandler = new FileSerializedDataHandler( Path.Combine( loadedVesselMetadata.GetRootDirectory(), "gameobjects.json" ), JsonFormat.Instance );

            HSPEvent.EventManager.TryInvoke( HSPEvent_BEFORE_DESIGN_SCENE_VESSEL_LOADED.ID, null );
            CurrentVesselMetadata = loadedVesselMetadata; // CurrentVesselMetadata should be set after invoking before load.

            // @@TODO - load the vessel from the 3-file state. Same as the gameplay load function.

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_DESIGN_SCENE_VESSEL_LOADED.ID, null );
        }

        // ------

        private static GameObject GetGameObject()
        {
            if( DesignObject == null || !DesignObject.Parts.Any() )
                throw new InvalidOperationException( $"Can't save, the design object is empty." );

            return DesignObject.gameObject;
        }
    }
}