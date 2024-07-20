using HSP.Timelines;
using HSP.Content.Timelines.Serialization;
using HSP.Vessels;
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
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            Directory.CreateDirectory( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" ) );

            int i = 0;
            foreach( var vessel in VesselManager.LoadedVessels )
            {
                GameObject vesselobject = vessel.gameObject;

                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels", $"{i}", "gameobjects.json" ) );

                var data = SerializationUnit.Serialize( vesselobject, TimelineManager.RefStore );
                _vesselsDataHandler.Write( data );
                i++;
            }
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD.ID, DESERIALIZE_VESSELS )]
        private static void OnLoad( TimelineManager.LoadEventData e )
        {
            string path = Path.Combine( SaveMetadata.GetRootDirectory( e.timelineId, e.saveId ), "Vessels" );
            Directory.CreateDirectory( path );

            foreach( var dir in Directory.GetDirectories( path ) )
            {
                JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ) );

                var data = _vesselsDataHandler.Read();
                var go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
            }
        }
    }
}