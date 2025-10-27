using HSP.Timelines;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
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

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveVesselManager )}.json" ) );

            var su = SerializationUnit.FromObjects( UnityEngine.Object.FindObjectOfType<ActiveVesselManager>() );
            SerializationResult result = su.Serialize( TimelineManager.RefStore );
#warning TODO - standardize the 'is failure' check in unityplus.
            if( result.HasFlag( SerializationResult.Failed ) || result.HasFlag( SerializationResult.HasFailures ) )
            {
                Debug.LogError( $"Failed to serialize ActiveVesselManager." );
                e.AddMessage( LogType.Error, $"Failed to serialize ActiveVesselManager." );
                return;
            }
            var data = su.GetData().First();
            dataHandler.Write( data );
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

            JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveVesselManager )}.json" ) );

            var data = dataHandler.Read();
            var su = SerializationUnit.PopulateObject( UnityEngine.Object.FindObjectOfType<ActiveVesselManager>(), data );
            SerializationResult result = su.Populate( TimelineManager.RefStore );
            if( result.HasFlag( SerializationResult.Failed ) || result.HasFlag( SerializationResult.HasFailures ) )
            {
                Debug.LogError( $"Failed to deserialize ActiveVesselManager." );
                e.AddMessage( LogType.Error, $"Failed to deserialize ActiveVesselManager." );
                return;
            }
        }
    }
}