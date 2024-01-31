using KSS.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.UI
{
    public class ControlSetupWindowNodeConnection
    {
        // connection is not a monobeh.

        ControlSetupControleeInput _from;
        ControlSetupControlerOutput _to;

        // outputs are events, inputs are methods. connections are delegates.
        Delegate _delegate;

        // also which controlin is connected to which controlout.

        internal static ControlSetupWindowNodeConnection Create( ControlSetupWindow window, ControlSetupControleeInput input, ControlSetupControlerOutput output )
        {
            // create

#warning TODo - check parameter compatibility. parameter list needs to be the same.

            // get and create niputs/outputs.

            ControlSetupWindowNodeConnection conn = new ControlSetupWindowNodeConnection();
            conn._from = input;
            conn._to = output;

#warning TODO - delegate.

            window.AddConnection( conn );

            return conn;
        }
    }
}