using HSP.CelestialBodies;
using HSP.SceneManagement;
using HSP.Timelines;
using HSP.Vessels;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Formats;

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
                var dataHandler = new FileSerializedDataHandler( Path.Combine( vesselsPath, $"{i}", "gameobjects.json" ), JsonFormat.Instance );

                try
                {
                    var data = SerializationUnit.Serialize( vessel.gameObject, TimelineManager.RefStore );
                    dataHandler.Write( data );
                }
                catch( UPSSerializationException ex )
                {
                    Debug.LogError( $"Failed to serialize vessel '{vessel.name}': {ex.Message}" );
                    Debug.LogException( ex );
                    e.AddMessage( LogType.Error, $"Failed to serialize vessel '{vessel.name}': {ex.Message}" );
                }
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
                var dataHandler = new FileSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ), JsonFormat.Instance );
                var data = dataHandler.Read();

                try
                {
                    GameObject go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
                    HSPSceneManager.MoveGameObjectToScene<GameplaySceneM>( go );
                }
                catch( UPSSerializationException ex )
                {
                    Debug.LogError( $"Failed to deserialize vessel from '{dir}'." );
                    Debug.LogException( ex );
                    e.AddMessage( LogType.Error, $"Failed to deserialize vessel from '{dir}'." );
                }
            }
        }
    }
}