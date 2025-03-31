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
        private static void SerializeVessels( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( e.save.GetRootDirectory(), "Vessels" ) );

            int i = 0;
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                JsonSerializedDataHandler vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( e.save.GetRootDirectory(), "Vessels", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( vessel.gameObject, TimelineManager.RefStore );
                vesselsDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_VESSELS )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_VESSELS )]
        private static void DeserializeVessels( object e )
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
                JsonSerializedDataHandler vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}