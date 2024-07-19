using HSP.Content.Timelines.Serialization;
using HSP.ReferenceFrames;
using HSP.Timelines;
using System.IO;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class OnTimelineSave
    {
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_SAVE.ID, HSPEvent.NAMESPACE_HSP + ".serialize_managers.active_object_manager" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            string savePath = SaveMetadata.GetRootDirectory( e.timelineId, e.saveId );
            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveObjectManager )}.json" ) );

            var data = SerializationUnit.Serialize( UnityEngine.Object.FindObjectOfType<ActiveObjectManager>(), TimelineManager.RefStore );
            _vesselsDataHandler.Write( data );
        }

        [HSPEventListener( HSPEvent_AFTER_TIMELINE_LOAD.ID, HSPEvent.NAMESPACE_HSP + ".deserialize_managers.active_object_manager" )]
        private static void OnLoad( TimelineManager.LoadEventData e )
        {
            string savePath = SaveMetadata.GetRootDirectory( e.timelineId, e.saveId );
            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveObjectManager )}.json" ) );

            var data = _vesselsDataHandler.Read();
            SerializationUnit.Populate( UnityEngine.Object.FindObjectOfType<ActiveObjectManager>(), data, TimelineManager.RefStore );
        }
    }
}