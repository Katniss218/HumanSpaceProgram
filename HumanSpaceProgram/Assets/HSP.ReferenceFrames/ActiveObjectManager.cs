using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.ReferenceFrames
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
    }
}