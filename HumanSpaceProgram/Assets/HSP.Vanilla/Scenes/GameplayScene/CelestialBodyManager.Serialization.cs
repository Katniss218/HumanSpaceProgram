using HSP.CelestialBodies;
using HSP.Timelines;
using System;
using System.IO;
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
        private static void SerializeCelestialBodies( object e )
        {
            string rootPath;
            if( e is TimelineManager.LoadEventData e2 )
                rootPath = Path.Combine( e2.save.GetRootDirectory(), "CelestialBodies" );
            else if( e is TimelineManager.StartNewEventData e3 )
                rootPath = Path.Combine( e3.scenario.GetRootDirectory(), "CelestialBodies" );
            else
                throw new ArgumentException();
            Directory.CreateDirectory( Path.Combine( rootPath, "CelestialBodies" ) );

            int i = 0;
            foreach( var celestialBody in CelestialBodyManager.CelestialBodies )
            {
                JsonSerializedDataHandler celestialBodiesDataHandler = new JsonSerializedDataHandler( Path.Combine( rootPath, "CelestialBodies", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( celestialBody.gameObject, TimelineManager.RefStore );
                celestialBodiesDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_CELESTIAL_BODIES )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_CELESTIAL_BODIES )]
        private static void DeserializeCelestialBodies( object e )
        {
            string rootPath;
            if( e is TimelineManager.LoadEventData e2 )
                rootPath = Path.Combine( e2.save.GetRootDirectory(), "CelestialBodies" );
            else if( e is TimelineManager.StartNewEventData e3 )
                rootPath = Path.Combine( e3.scenario.GetRootDirectory(), "CelestialBodies" );
            else
                throw new ArgumentException();

            if( !Directory.Exists( rootPath ) )
                return;

            foreach( var dir in Directory.GetDirectories( rootPath ) )
            {
                JsonSerializedDataHandler celestialBodiesDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = celestialBodiesDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}