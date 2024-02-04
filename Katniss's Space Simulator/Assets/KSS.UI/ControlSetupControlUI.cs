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
    public class ControlSetupControlUI : MonoBehaviour
    {
        public const float HEIGHT = 15.0f;

        ControlSetupControlGroupUI _group;

        ControlSetupControlUI _connectedTo;

        public Control.Control Control { get; private set; }
        NamedControlAttribute _attr;

        // output of a control channel

        // on release on this, when dragging out a connection - connect.

        // show the name/description of channel on mouseover.
        // also when pressed if connected to something - disconnect and hook the end to the mouse until released. if released over nothing - delete connection

        internal static ControlSetupControlUI Create( ControlSetupControlGroupUI group, float side, float verticalOffset, Control.Control control, NamedControlAttribute attr )
        {
            UIPanel panel = group.panel.AddPanel( UILayoutInfo.FillHorizontal( 0, 0, UILayoutInfo.TopF, -verticalOffset, HEIGHT ), null );
            UIIcon icon = panel.AddIcon( new UILayoutInfo( new Vector2( side, 1.0f ), Vector2.zero, new Vector2( HEIGHT, HEIGHT ) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" ) );

            UIText name = panel.AddText( UILayoutInfo.FillHorizontal( (1 - side) * HEIGHT, (side) * HEIGHT, UILayoutInfo.TopF, 0, HEIGHT ), attr.Name )
                .WithAlignment( side == 0 ? TMPro.HorizontalAlignmentOptions.Left : TMPro.HorizontalAlignmentOptions.Right );

            ControlSetupControlUI output = icon.gameObject.AddComponent<ControlSetupControlUI>();
            output._group = group;
            output.Control = control;
            output._attr = attr;

            return output;
        }
    }
}