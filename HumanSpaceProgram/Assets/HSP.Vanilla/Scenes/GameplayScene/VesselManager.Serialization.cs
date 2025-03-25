using HSP.Timelines;
using HSP.Vessels;
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
        private static void SerializeVessels( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( e.save.GetRootDirectory(), "Vessels" ) );

            int i = 0;
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( e.save.GetRootDirectory(), "Vessels", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( vessel.gameObject, TimelineManager.RefStore );
                _vesselsDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_VESSELS )]
        private static void DeserializeVessels( TimelineManager.LoadEventData e )
        {
            string path = Path.Combine( e.save.GetRootDirectory(), "Vessels" );
            if( !Directory.Exists( path ) )
                return;

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = _vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_VESSELS )]
        private static void DeserializeScenarioVessels( TimelineManager.StartNewEventData e )
        {
            string path = Path.Combine( e.scenario.GetRootDirectory(), "Vessels" );
            if( !Directory.Exists( path ) )
                return;

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = _vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}