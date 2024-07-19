using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ReferenceFrames
{
    public static class HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED
    {
        /// <summary>
        /// Invoked after the active object changes.
        /// </summary>
        public const string ID = HSPEvent.NAMESPACE_HSP + ".after_active_object_changed";
    }

    /// <summary>
    /// Manages the currently active object.
    /// </summary>
    public class ActiveObjectManager : SingletonMonoBehaviour<ActiveObjectManager>
    {
        [SerializeField]
        private Transform _activeObject;
        /// <summary>
        /// Gets or sets the object that is currently being 'controlled' or viewed by the player.
        /// </summary>
        public static Transform ActiveObject
        {
            get => instance._activeObject;
            set
            {
                if( value == instance._activeObject )
                    return;
                instance._activeObject = value;
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID, null );
            }
        }

        [MapsInheritingFrom( typeof( ActiveObjectManager ) )]
        public static SerializationMapping ActiveObjectManagerMapping()
        {
            return new MemberwiseSerializationMapping<ActiveObjectManager>()
            {
                ("active_object", new Member<ActiveObjectManager, Transform>( ObjectContext.Ref, o => ActiveObjectManager.ActiveObject, (o, value) => ActiveObjectManager.ActiveObject = value ))
            };
        }
    }
}