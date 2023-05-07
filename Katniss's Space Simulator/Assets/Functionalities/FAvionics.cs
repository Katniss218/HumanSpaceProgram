using KatnisssSpaceSimulator.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatnisssSpaceSimulator.Functionalities
{
    public class FAvionics : Functionality
    {
        // Avionics would have the task of translating whatever inputs (user/autopilot/etc) it receives, into control sigmals for each control channel.
        // then whatever is hooked up to the channels will interpret the signals.

        // the signal will depend on the part being connected. for example if the RCS is connected, the signal should be different for RCS positioned in different directions, and in different ways.


        public override void Load( JToken data )
        {
            throw new NotImplementedException();
        }

        public override JToken Save()
        {
            throw new NotImplementedException();
        }
    }
}