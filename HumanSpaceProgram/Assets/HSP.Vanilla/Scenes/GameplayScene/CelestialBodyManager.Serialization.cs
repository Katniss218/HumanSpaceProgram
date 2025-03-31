using HSP.CelestialBodies;
using HSP.Timelines;
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
        private static void SerializeCelestialBodies( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( e.save.GetRootDirectory(), "CelestialBodies" ) );

            int i = 0;
            foreach( var celestialBody in CelestialBodyManager.CelestialBodies )
            {
                JsonSerializedDataHandler celestialBodiesDataHandler = new JsonSerializedDataHandler( Path.Combine( e.save.GetRootDirectory(), "CelestialBodies", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( celestialBody.gameObject, TimelineManager.RefStore );
                celestialBodiesDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_CELESTIAL_BODIES )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_CELESTIAL_BODIES )]
        private static void DeserializeCelestialBodies( object e )
        {
            string path;
            if( e is TimelineManager.LoadEventData e2 )
                path = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is TimelineManager.StartNewEventData e3 )
                path = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            if( !Directory.Exists( path ) )
                return;

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler celestialBodiesDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = celestialBodiesDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}