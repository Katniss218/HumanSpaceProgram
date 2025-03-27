using HSP.CelestialBodies;
using HSP.Timelines;
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
        private static void SerializeCelestialBodies( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( e.save.GetRootDirectory(), "CelestialBodies" ) );

            int i = 0;
            foreach( var celestialBody in CelestialBodyManager.CelestialBodies )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( e.save.GetRootDirectory(), "CelestialBodies", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( celestialBody.gameObject, TimelineManager.RefStore );
                _vesselsDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_CELESTIAL_BODIES )]
        private static void DeserializeCelestialBodies( TimelineManager.LoadEventData e )
        {
            string path = Path.Combine( e.save.GetRootDirectory(), "CelestialBodies" );
            if( !Directory.Exists( path ) )
                return;

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = _vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }

        /*[HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_CELESTIAL_BODIES )]
        private static void DeserializeScenarioCelestialBodies( TimelineManager.StartNewEventData e )
        {
            string path = Path.Combine( e.scenario.GetRootDirectory(), "CelestialBodies" );
            if( !Directory.Exists( path ) )
                return;

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = _vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }*/
    }
}