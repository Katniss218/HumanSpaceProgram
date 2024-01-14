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
    public class ControlSetupWindowNodeOutput : MonoBehaviour
    {
        ControlSetupWindowNode _node;

        // outputs are events, inputs are methods. connections are delegates.
        EventInfo _event;
        ControlOutAttribute _attr;

        // output of a control channel

        // on release on this, when dragging out a connection - connect.

        // show the name/description of channel on mouseover.
        // also when pressed if connected to something - disconnect and hook the end to the mouse until released. if released over nothing - delete connection

        internal static ControlSetupWindowNodeOutput Create( ControlSetupWindowNode node, float verticalOffset, EventInfo member, ControlOutAttribute attr )
        {
            UIIcon icon = node.panel.AddIcon( new UILayoutInfo( UILayoutInfo.TopRight, UILayoutInfo.TopRight, new Vector2( 0, verticalOffset ), new Vector2( 10, 10 ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" ) );

            ControlSetupWindowNodeOutput output = icon.gameObject.AddComponent<ControlSetupWindowNodeOutput>();
            output._node = node;
            output._event = member;
            output._attr = attr;

            return output;
        }
    }
}