using HSP.CelestialBodies;
using HSP.SceneManagement;
using HSP.Timelines;
using System;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Formats;

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
                var dataHandler = new FileSerializedDataHandler( Path.Combine( celestialBodiesPath, $"{i}", "gameobjects.json" ), JsonFormat.Instance );

                try
                {
                    var data = SerializationUnit.Serialize( celestialBody.gameObject, TimelineManager.RefStore );
                    dataHandler.Write( data );
                }
                catch( UPSSerializationException ex )
                {
                    Debug.LogError( $"Failed to serialize celestial body '{celestialBody.name}': {ex.Message}" );
                    Debug.LogException( ex );
                    e.AddMessage( LogType.Error, $"Failed to serialize celestial body '{celestialBody.name}': {ex.Message}" );
                }
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
                var dataHandler = new FileSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ), JsonFormat.Instance );
                var data = dataHandler.Read();

                try
                {
                    GameObject go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
                    HSPSceneManager.MoveGameObjectToScene<GameplaySceneM>( go );
                }
                catch( UPSSerializationException ex )
                {
                    Debug.LogError( $"Failed to deserialize celestial body from '{dir}'." );
                    Debug.LogException( ex );
                    e.AddMessage( LogType.Error, $"Failed to deserialize celestial body from '{dir}'." );
                }
            }
        }
    }
}