using HSP.SceneManagement;
using HSP.Timelines;
using HSP.Vessels;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class VesselManager_Serialization
    {
        public const string SERIALIZE_VESSELS = HSPEvent.NAMESPACE_HSP + ".serialize_vessels";
        public const string DESERIALIZE_VESSELS = HSPEvent.NAMESPACE_HSP + ".deserialize_vessels";

        [HSPEventListener( HSPEvent_ON_TIMELINE_SAVE.ID, SERIALIZE_VESSELS )]
        [HSPEventListener( HSPEvent_ON_SCENARIO_SAVE.ID, SERIALIZE_VESSELS )]
        private static void SerializeVessels( IMessageEventData e )
        {
            string vesselsPath;
            if( e is TimelineSaveEventData e2 )
                vesselsPath = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is ScenarioSaveEventData e3 )
                vesselsPath = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            Directory.CreateDirectory( vesselsPath );

            int i = 0;
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( vesselsPath, $"{i}", "gameobjects.json" ) );

                var su = SerializationUnit.FromObjects( vessel.gameObject );
                SerializationResult result = su.Serialize( TimelineManager.RefStore );
                if( result.HasFlag( SerializationResult.Failed ) )
                {
                    Debug.LogError( $"Failed to serialize vessel '{vessel.name}'." );
                    e.AddMessage( LogType.Error, $"Failed to serialize vessel '{vessel.name}'." );
                    continue;
                }
                var data = su.GetData().First();
                dataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_VESSELS, After = new[] { CelestialBodyManager_Serialization.DESERIALIZE_CELESTIAL_BODIES } )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_VESSELS, After = new[] { CelestialBodyManager_Serialization.DESERIALIZE_CELESTIAL_BODIES } )]
        private static void DeserializeVessels( IMessageEventData e )
        {
            string vesselsPath;
            if( e is TimelineLoadEventData e2 )
                vesselsPath = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is TimelineNewEventData e3 )
                vesselsPath = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            if( !Directory.Exists( vesselsPath ) )
                return;

            foreach( var dir in Directory.GetDirectories( vesselsPath ) )
            {
                JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                SerializedData data = dataHandler.Read();
                var su = SerializationUnit.FromData<GameObject>( data );
                SerializationResult result = su.Deserialize( TimelineManager.RefStore );
                if( result.HasFlag( SerializationResult.Failed ) )
                {
                    Debug.LogError( $"Failed to deserialize vessel from '{dir}'." );
                    e.AddMessage( LogType.Error, $"Failed to deserialize vessel from '{dir}'." );
                    continue;
                }
                GameObject go = su.GetObjects().First();
                HSPSceneManager.MoveGameObjectToScene<GameplaySceneM>( go );
            }
        }
    }
}