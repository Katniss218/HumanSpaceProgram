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

        ControlSetupControlUI _output; // from.
        ControlSetupControlUI _input; // to.

        // also which controlin is connected to which controlout.

        internal static bool TryCreate( ControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output, out ControlSetupWindowNodeConnection connection )
        {
            if( !output.Control.TryConnect( input.Control ) )
            {
                connection = null;
                return false;
            }

            connection = new ControlSetupWindowNodeConnection();
            connection._input = input;
            connection._output = output;

            window.AddConnection( connection );

            return true;
        }
    }
}