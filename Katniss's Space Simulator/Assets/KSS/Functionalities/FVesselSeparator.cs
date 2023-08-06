using KSS.Core;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Functionalities
{
    public class FVesselSeparator : MonoBehaviour, IPersistent
    {
        Vessel v;
        Part p;

        bool _separated = false;

        void Start()
        {
            p = this.GetComponent<Part>();
            v = this.transform.parent.GetComponent<Vessel>();
        }

        void Update()
        {
            if( _separated )
            {
                return;
            }
            if( Input.GetKeyDown( KeyCode.Space ) )
            {
#warning TODO - disconnect pipes, and stuff. Probably event based.

                VesselStateUtils.SetParent( p, null );
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