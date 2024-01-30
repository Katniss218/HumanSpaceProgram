using KSS.Control;
using KSS.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class ControlSetupWindowNode : MonoBehaviour
    {
        Component _component; // the comp being represented by this node.

        ControlSetupWindowNodeInput[] _inputs;
        ControlSetupWindowNodeOutput[] _outputs;

        internal UIPanel panel; // this is ugly. UI should provide a way of creating new UI elements easily, without boilerplate, and with restrictions what can be a child, etc.

        // maybe add groups?

        /// <summary>
        /// Creates a control setup node for a given component.
        /// </summary>
        internal static ControlSetupWindowNode Create( ControlSetupWindow window, Component component )
        {
            // it is possible to force-show nodes for components outside of the target hierarchy of the window.

            IEnumerable<(Control.Control member, NamedControlAttribute attr)> controls = ControlUtils.GetControls( component );

            UIPanel panel = window.window.AddPanel( new UILayoutInfo( UILayoutInfo.Middle, Vector2.zero, new Vector2( 50, 50 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_node" ) );

            ControlSetupWindowNode node = panel.gameObject.AddComponent<ControlSetupWindowNode>();
            node.panel = panel;
            //node._inputs = inputs.Select( (input, i) => ControlSetupWindowNodeInput.Create( node, i * 10, input.member, input.attr ) ).ToArray();
            //node._outputs = outputs.Select( (output, i) => ControlSetupWindowNodeOutput.Create( node, i * 10, output.member, output.attr ) ).ToArray();

            return node;
        }
    }
}