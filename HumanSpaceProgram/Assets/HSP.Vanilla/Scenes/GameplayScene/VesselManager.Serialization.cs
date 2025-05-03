using HSP.Timelines;
using HSP.Vessels;
using System;
using System.IO;
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
        private static void SerializeVessels( object e )
        {
            string vesselsPath;
            if( e is TimelineManager.SaveEventData e2 )
                vesselsPath = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is TimelineManager.SaveScenarioEventData e3 )
                vesselsPath = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            Directory.CreateDirectory( vesselsPath );

            int i = 0;
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( vesselsPath, $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( vessel.gameObject, TimelineManager.RefStore );
                dataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_NEW.ID, DESERIALIZE_VESSELS )]
        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_VESSELS )]
        private static void DeserializeVessels( object e )
        {
            string vesselsPath;
            if( e is TimelineManager.LoadEventData e2 )
                vesselsPath = Path.Combine( e2.save.GetRootDirectory(), "Vessels" );
            else if( e is TimelineManager.StartNewEventData e3 )
                vesselsPath = Path.Combine( e3.scenario.GetRootDirectory(), "Vessels" );
            else
                throw new ArgumentException();

            if( !Directory.Exists( vesselsPath ) )
                return;

            foreach( var dir in Directory.GetDirectories( vesselsPath ) )
            {
                JsonSerializedDataHandler dataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = dataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}