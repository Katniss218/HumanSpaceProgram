﻿using System;
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
    public class ActiveObjectManager : SingletonMonoBehaviour<ActiveObjectManager>, IPersistent
    {
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