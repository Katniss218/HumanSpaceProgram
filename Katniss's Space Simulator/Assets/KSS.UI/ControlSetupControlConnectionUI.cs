using KSS.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.UI
{
    public class ControlSetupControlConnectionUI
    {
        // connection is not a monobeh.

        ControlSetupControlUI _from;
        ControlSetupControlUI _to;

        // also which controlin is connected to which controlout.

        // connection can be connected to a control at one end, and to the mouse at the other.

        // connections are directional (for visual design reasons)

        // we really need 2 types of controls (inputs and outputs) for better typing. The distinction between actions and params isn't needed.

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        internal static bool TryCreate( ControlSetupWindow window, ControlSetupControlUI input, ControlSetupControlUI output, out ControlSetupControlConnectionUI connection )
        {
            if( !output.Control.TryConnect( input.Control ) )
            {
                connection = null;
                return false;
            }

            connection = new ControlSetupControlConnectionUI();
            connection._to = input;
            connection._from = output;

            return true;
        }
    }
}