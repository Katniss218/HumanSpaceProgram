using HSP.Core.Components;
using HSP.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Core
{
    /// <summary>
    /// Manages the currently active object.
    /// </summary>
    public class ActiveObjectManager : SingletonMonoBehaviour<ActiveObjectManager>
    {
        [SerializeField]
        private GameObject _activeObject;
        /// <summary>
        /// Gets or sets the object that is currently being 'controlled' or viewed by the player.
        /// </summary>
        public static GameObject ActiveObject
        {
            get => instance._activeObject;
            set
            {
                if( value == instance._activeObject )
                    return;
                instance._activeObject = value;
                HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, null );
            }
        }

        [MapsInheritingFrom( typeof( ActiveObjectManager ) )]
        public static SerializationMapping ActiveObjectManagerMapping()
        {
            return new MemberwiseSerializationMapping<ActiveObjectManager>()
            {
                ("active_object", new Member<ActiveObjectManager, GameObject>( ObjectContext.Ref, o => ActiveObjectManager.ActiveObject, (o, value) => ActiveObjectManager.ActiveObject = value ))
            };
        }

        [HSPEventListener( HSPEvent.TIMELINE_AFTER_SAVE, HSPEvent.NAMESPACE_VANILLA + ".serialize_managers.active_object_manager" )]
        private static void OnBeforeSave( TimelineManager.SaveEventData e )
        {
            string savePath = SaveMetadata.GetRootDirectory( e.timelineId, e.saveId );
            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveObjectManager )}.json" ) );

            var data = SerializationUnit.Serialize( FindObjectOfType<ActiveObjectManager>(), TimelineManager.RefStore );
            _vesselsDataHandler.Write( data );
        }

        [HSPEventListener( HSPEvent.TIMELINE_AFTER_LOAD, HSPEvent.NAMESPACE_VANILLA + ".deserialize_managers.active_object_manager" )]
        private static void OnLoad( TimelineManager.LoadEventData e )
        {
            string savePath = SaveMetadata.GetRootDirectory( e.timelineId, e.saveId );
            Directory.CreateDirectory( savePath );

            JsonSerializedDataHandler _vesselsDataHandler = new JsonSerializedDataHandler( Path.Combine( savePath, $"{nameof( ActiveObjectManager )}.json" ) );

            var data = _vesselsDataHandler.Read();
            SerializationUnit.Populate( FindObjectOfType<ActiveObjectManager>(), data, TimelineManager.RefStore );
        }
    }
}