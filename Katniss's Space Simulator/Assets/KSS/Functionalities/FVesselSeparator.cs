using KSS.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public void SetData( Loader l, SerializedData data )
        {
            throw new NotImplementedException();
        }

        public SerializedData GetData( Saver s )
        {
            throw new NotImplementedException();
        }
        public JToken Save( int fileVersion )
        {
            switch( fileVersion )
            {
                case 0:
                    return new JObject()
                    {
                        { "separated", this._separated }
                    };
                default:
                    return new JObject();
            }
        }

        public void Load( int fileVersion, JToken data )
        {
            switch( fileVersion )
            {
                case 0:
                    this._separated = (bool)data["separated"];
                    return;
                default:
                    return;
            }
        }
    }
}