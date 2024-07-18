using HSP.ReferenceFrames;
using HSP.Timelines.Serialization;
using HSP.Timelines;
using HSP;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    public static class OnTimelineSave
    {
        [HSPEventListener( HSPEvent.TIMELINE_AFTER_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_managers.active_object_manager" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            string savePath = SaveMetadata.GetRootDirectory( e.timelineId, e.saveId );
            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveObjectManager )}.json" ) );

            var data = SerializationUnit.Serialize( UnityEngine.Object.FindObjectOfType<ActiveObjectManager>(), TimelineManager.RefStore );
            _vesselsDataHandler.Write( data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_AFTER_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_managers.active_object_manager" )]
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