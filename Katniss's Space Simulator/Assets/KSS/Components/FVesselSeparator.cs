using KSS.Core;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class FVesselSeparator : MonoBehaviour, IPersistsData
    {
        bool _hasSeparated = false;

        void Update()
        {
            if( _hasSeparated )
            {
                return;
            }

            if( UnityEngine.Input.GetKeyDown( KeyCode.Space ) ) // todo - use control systems instead.
            {
#warning TODO - disconnect pipes, and stuff. Use 'OnVesselSeparate' and 'OnVesselJoin' events.

                VesselHierarchyUtils.SetParent( this.transform, null );
                _hasSeparated = true;
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "has_separated", this._hasSeparated }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "has_separated", out var hasSeparated ) )
                this._hasSeparated = (bool)hasSeparated;
        }
    }
}