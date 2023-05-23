using KSS.Core.Serialization;
using KSS.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSS.Control;

namespace KSS.Functionalities
{
    public class FAvionics : MonoBehaviour, IPersistent
    {
        // Avionics would have the task of translating whatever inputs (user/autopilot/etc) it receives, into control sigmals for each control channel.
        // then whatever is hooked up to the channels will interpret the signals.

        // the signal will depend on the part being connected. for example if the RCS is connected, the signal should be different for RCS positioned in different directions, and in different ways.
        
        // something like this. these could then be further connected.
        [ControlOut( "steer.x", "Steer X" )]
        public event Action<float> SteerX;

        [ControlOut( "steer.y", "Steer Y" )]
        public event Action<float> SteerY;


        public JToken Save( int fileVersion )
        {
            throw new NotImplementedException();
        }

        public void Load( int fileVersion, JToken data )
        {
            throw new NotImplementedException();
        }
    }
}