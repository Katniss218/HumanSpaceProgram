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
            return new SerializedObject()
            {
                { "has_separated", this._hasSeparated }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "has_separated", out var hasSeparated ) )
                this._hasSeparated = (bool)hasSeparated;
        }
    }
}