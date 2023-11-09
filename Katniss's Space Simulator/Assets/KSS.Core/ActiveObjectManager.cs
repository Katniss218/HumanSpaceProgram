using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public class ActiveObjectManager : SingletonMonoBehaviour<ActiveObjectManager>, IPersistent
    {
        private GameObject _activeObject;
        public static GameObject ActiveObject
        {
            get => instance._activeObject;
            set
            {
                instance._activeObject = value;
                HSPEvent.EventManager.TryInvoke( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, null );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "active_object", s.WriteObjectReference( ActiveObject ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "active_object", out var activeObject ) )
                ActiveObject = (GameObject)l.ReadObjectReference( activeObject );
        }
    }
}