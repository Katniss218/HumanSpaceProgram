using HSP.Timelines;
using System;
using System.IO;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class ActiveVesselManager_Serialization
    {
        public const string SERIALIZE_ACTIVE_OBJECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".serialize_active_object_manager";
        public const string DESERIALIZE_ACTIVE_OBJECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".deserialize_active_object_manager";

        [HSPEventListener( HSPEvent_AFTER_SCENARIO_SAVE.ID, SERIALIZE_ACTIVE_OBJECT_MANAGER )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_SAVE.ID, SERIALIZE_ACTIVE_OBJECT_MANAGER )]
        private static void OnBeforeSave( object e )
        {
            string savePath;
            if( e is TimelineManager.SaveEventData e2 )
                savePath = e2.save.GetRootDirectory();
            else if( e is TimelineManager.SaveScenarioEventData e3 )
                savePath = e3.scenario.GetRootDirectory();
            else
                throw new ArgumentException();

            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveVesselManager )}.json" ) );

            var data = SerializationUnit.Serialize( UnityEngine.Object.FindObjectOfType<ActiveVesselManager>(), TimelineManager.RefStore );
            dataHandler.Write( data );
        }

        [HSPEventListener( HSPEvent_AFTER_TIMELINE_NEW.ID, DESERIALIZE_ACTIVE_OBJECT_MANAGER )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_LOAD.ID, DESERIALIZE_ACTIVE_OBJECT_MANAGER )]
        private static void OnLoad( object e )
        {
            string savePath;
            if( e is TimelineManager.LoadEventData e2 )
                savePath = e2.save.GetRootDirectory();
            else if( e is TimelineManager.StartNewEventData e3 )
                savePath = e3.scenario.GetRootDirectory();
            else
                throw new ArgumentException();

            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveVesselManager )}.json" ) );

            var data = dataHandler.Read();
            SerializationUnit.Populate( UnityEngine.Object.FindObjectOfType<ActiveVesselManager>(), data, TimelineManager.RefStore );
        }
    }
}