using KSS.Control;
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
    public class ControlSetupWindowNodeInput : MonoBehaviour
    {
        ControlSetupWindowNode _node;

        // outputs are events, inputs are methods. connections are delegates.
        Control.Control _method;
        NamedControlAttribute _attr;

        // input to a control channel

        // on click on this, start dragging out a connection.

        // show the name/description of channel on mouseover.

        // also when pressed if connected to something - disconnect and hook the end to the mouse until released. if released over nothing - delete connection

        internal static ControlSetupWindowNodeInput Create( ControlSetupWindowNode node, float verticalOffset, Control.Control member, NamedControlAttribute attr )
        {
            // create a circle "button", maybe with an arrow pointing right (inputs appear on the left side)

            UIIcon icon = node.panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopLeft, UILayoutInfo.TopLeft, new Vector2( 0, verticalOffset ), new Vector2( 10, 10 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_input" ) );

            ControlSetupWindowNodeInput input = icon.gameObject.AddComponent<ControlSetupWindowNodeInput>();
            input._node = node;
            input._method = member;
            input._attr = attr;

            return input;
        }
    }
}