using HSP.CelestialBodies;
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
    public static class CelestialBodyManager_Serialization
    {
        public const string SERIALIZE_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".serialize_celestial_bodies";
        public const string DESERIALIZE_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".deserialize_celestial_bodies";

        [HSPEventListener( HSPEvent_ON_TIMELINE_SAVE.ID, SERIALIZE_CELESTIAL_BODIES )]
        [HSPEventListener( HSPEvent_ON_SCENARIO_SAVE.ID, SERIALIZE_CELESTIAL_BODIES )]
        private static void SerializeCelestialBodies( IMessageEventData e )
        {
            string celestialBodiesPath;
            if( e is TimelineSaveEventData e2 )
                celestialBodiesPath = Path.Combine( e2.save.GetRootDirectory(), "CelestialBodies" );
            else if( e is ScenarioSaveEventData e3 )
                celestialBodiesPath = Path.Combine( e3.scenario.GetRootDirectory(), "CelestialBodies" );
            else
                throw new ArgumentException();

            Directory.CreateDirectory( celestialBodiesPath );

            int i = 0;
            foreach( var celestialBody in CelestialBodyManager.CelestialBodies )
            {
                JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( celestialBodiesPath, $"{i}", "gameobjects.json" ) );

                var su = SerializationUnit.FromObjects( celestialBody.gameObject );
                SerializationResult result = su.Serialize( TimelineManager.RefStore );
                if( result.HasFlag( SerializationResult.Failed ) || result.HasFlag( SerializationResult.HasFailures ) )
                {
                    Debug.LogError( $"Failed to serialize celestial body '{celestialBody.name}'." );
                    e.AddMessage( LogType.Error, $"Failed to serialize celestial body '{celestialBody.name}'." );
                    continue;
                }
                var data = su.GetData().First();
                dataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_CELESTIAL_BODIES )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_CELESTIAL_BODIES )]
        private static void DeserializeCelestialBodies( IMessageEventData e )
        {
            string celestialBodiesPath;
            if( e is TimelineLoadEventData e2 )
                celestialBodiesPath = Path.Combine( e2.save.GetRootDirectory(), "CelestialBodies" );
            else if( e is TimelineNewEventData e3 )
                celestialBodiesPath = Path.Combine( e3.scenario.GetRootDirectory(), "CelestialBodies" );
            else
                throw new ArgumentException();

            if( !Directory.Exists( celestialBodiesPath ) )
                return;

            foreach( var dir in Directory.GetDirectories( celestialBodiesPath ) )
            {
                JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = dataHandler.Read();
                var su = SerializationUnit.FromData<GameObject>( data );
                SerializationResult result = su.Deserialize( TimelineManager.RefStore );
                if( result.HasFlag( SerializationResult.Failed ) || result.HasFlag( SerializationResult.HasFailures ) )
                {
                    Debug.LogError( $"Failed to deserialize celestial body from '{dir}'." );
                    e.AddMessage( LogType.Error, $"Failed to deserialize celestial body from '{dir}'." );
                    continue;
                }
                GameObject go = su.GetObjects().First();
                HSPSceneManager.MoveGameObjectToScene<GameplaySceneM>( go );
            }
        }
    }
}