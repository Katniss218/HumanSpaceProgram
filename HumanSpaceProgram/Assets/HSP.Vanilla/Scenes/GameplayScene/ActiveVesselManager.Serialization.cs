using HSP.Time;
using HSP.Timelines;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Formats;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class ActiveVesselManager_Serialization
    {
        public const string SERIALIZE_ACTIVE_OBJECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".serialize_active_object_manager";
        public const string DESERIALIZE_ACTIVE_OBJECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".deserialize_active_object_manager";

        [HSPEventListener( HSPEvent_AFTER_SCENARIO_SAVE.ID, SERIALIZE_ACTIVE_OBJECT_MANAGER )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_SAVE.ID, SERIALIZE_ACTIVE_OBJECT_MANAGER )]
        private static void OnBeforeSave( IMessageEventData e )
        {
            string savePath;
            if( e is TimelineSaveEventData e2 )
                savePath = e2.save.GetRootDirectory();
            else if( e is ScenarioSaveEventData e3 )
                savePath = e3.scenario.GetRootDirectory();
            else
                throw new ArgumentException();

            Directory.CreateDirectory( savePath );

            var dataHandler = new FileSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveVesselManager )}.json" ), JsonFormat.Instance );

            try
            {
                var data = SerializationUnit.Serialize( UnityEngine.Object.FindObjectOfType<ActiveVesselManager>(), TimelineManager.RefStore );
                dataHandler.Write( data );
            }
            catch( UPSSerializationException ex )
            {
                Debug.LogError( $"Failed to serialize ActiveVesselManager: {ex.Message}" );
                Debug.LogException( ex );
                e.AddMessage( LogType.Error, $"Failed to serialize ActiveVesselManager: {ex.Message}" );
            }
        }

        [HSPEventListener( HSPEvent_AFTER_TIMELINE_NEW.ID, DESERIALIZE_ACTIVE_OBJECT_MANAGER )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_LOAD.ID, DESERIALIZE_ACTIVE_OBJECT_MANAGER )]
        private static void OnLoad( IMessageEventData e )
        {
            string savePath;
            if( e is TimelineLoadEventData e2 )
                savePath = e2.save.GetRootDirectory();
            else if( e is TimelineNewEventData e3 )
                savePath = e3.scenario.GetRootDirectory();
            else
                throw new ArgumentException();

            Directory.CreateDirectory( savePath );

            var dataHandler = new FileSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveVesselManager )}.json" ), JsonFormat.Instance );
            var data = dataHandler.Read();

            try
            {
                SerializationUnit.Populate<ActiveVesselManager>( UnityEngine.Object.FindObjectOfType<ActiveVesselManager>(), data, TimelineManager.RefStore );
            }
            catch( UPSSerializationException ex )
            {
                Debug.LogError( $"Failed to deserialize ActiveVesselManager: {ex.Message}" );
                Debug.LogException( ex );
                e.AddMessage( LogType.Error, $"Failed to deserialize ActiveVesselManager: {ex.Message}" );
            }
        }
    }
}