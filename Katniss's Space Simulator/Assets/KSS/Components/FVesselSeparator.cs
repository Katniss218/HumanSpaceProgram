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

        bool _separated = false;

        void Start()
        {
            p = this.transform;
            v = this.transform.GetVessel();
        }

        void Update()
        {
            if( _separated )
            {
                return;
            }
            if( Input.GetKeyDown( KeyCode.Space ) )
            {
#warning TODO - disconnect pipes, and stuff. Use 'OnVesselSeparate' and 'OnVesselJoin' events.

                VesselHierarchyUtils.SetParent( p, null );
            }
        }


        public void SetData( ILoader l, SerializedData data )
        {
            throw new NotImplementedException();
        }

        public SerializedData GetData( ISaver s )
        {
            throw new NotImplementedException();
        }
    }
}