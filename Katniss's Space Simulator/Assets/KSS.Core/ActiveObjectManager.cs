using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
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

        [SerializationMappingProvider( typeof( ActiveObjectManager ) )]
        public static SerializationMapping ActiveObjectManagerMapping()
        {
            return new CompoundSerializationMapping<ActiveObjectManager>()
            {
                ("active_object", new MemberReference<ActiveObjectManager, GameObject>( o => ActiveObjectManager.ActiveObject, (o, value) => ActiveObjectManager.ActiveObject = value ))
            }
            .IncludeMembers<Behaviour>()
            .UseBaseTypeFactory();
        }
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "active_object", s.WriteObjectReference( ActiveObject ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "active_object", out var activeObject ) )
                ActiveObject = (GameObject)l.ReadObjectReference( activeObject );
        }*/
    }
}