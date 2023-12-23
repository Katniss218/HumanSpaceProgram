using KSS.Core;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    public class FVesselSeparator : MonoBehaviour, IPersistent
    {
        Vessel v;
        Transform p;

        bool _hasSeparated = false;

        void Start()
        {
            p = this.transform;
            v = this.transform.GetVessel();
        }

        void Update()
        {
            if( _hasSeparated )
            {
                return;
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.Space ) )
            {
#warning TODO - disconnect pipes, and stuff. Use 'OnVesselSeparate' and 'OnVesselJoin' events.

                VesselHierarchyUtils.SetParent( p, null );
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