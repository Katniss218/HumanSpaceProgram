using KSS.Control;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Functionalities
{
    [Obsolete( "It's a prototype" )]
    public class FController : MonoBehaviour, IPersistent
    {
        // Avionics would have the task of translating whatever inputs (user/autopilot/etc) it receives, into control sigmals for each control channel.
        // then whatever is hooked up to the channels will interpret the signals.

        // the signal will depend on the part being connected. for example if the RCS is connected, the signal should be different for RCS positioned in different directions, and in different ways.

        // something like this. these could then be further connected.
        [ControlOut( "steer.x", "Steer X" )]
        public event Action<float> SteerX;

        [ControlOut( "steer.y", "Steer Y" )]
        public event Action<float> SteerY;


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