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
        public RectTransform Circle { get; private set; }

        public ControlSetupControlGroupUI Group { get; private set; }
        public Control.Control Control { get; private set; }

        /// <summary>
        /// 0 or 1, depending on which side the endpoint is drawn on.
        /// </summary>
        public float Side { get; private set; }

        NamedControlAttribute _attr;

        static ControlSetupControlUI _startedConnection;

        void OnClick()
        {
            // on release on this, when dragging out a connection - connect.

            // show the name/description of channel on mouseover.
            // also when pressed if connected to something - disconnect and hook the end to the mouse until released. if released over nothing - delete connection
            if( _startedConnection == null )
                _startedConnection = this;
            else
            {
                if( _startedConnection != this )
                {
                    Group.ComponentUI.Window.TryConnectWithMouse( _startedConnection, this );
                }
                _startedConnection = null;
            }
        }

        internal static ControlSetupControlUI Create( ControlSetupControlGroupUI group, float verticalOffset, Control.Control control, NamedControlAttribute attr )
        {
            UIPanel panel = group.panel.AddPanel( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, -verticalOffset, ControlSetupControlGroupUI.ROW_HEIGHT ), null );

            ControlSetupControlUI controlUI = panel.gameObject.AddComponent<ControlSetupControlUI>();
            controlUI.Group = group;
            controlUI.Control = control;
            controlUI._attr = attr;

            float side;
            Sprite sprite;
            switch( control )
            {
                case KSS.Control.Controls.ControlleeInput:
                    sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_input" ); side = 0f; break;
                case KSS.Control.Controls.ControllerOutput:
                    sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_output" ); side = 1f; break;
                case KSS.Control.Controls.ControlParameterInput:
                    sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_parameter_input" ); side = 1f; break;
                case KSS.Control.Controls.ControlParameterOutput:
                    sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/control_parameter_output" ); side = 0f; break;
                default:
                    sprite = null;
                    side = 0f;
                    break;
            }

            UIButton button = panel.AddButton( new UILayoutInfo( (side, 1.0f), (0, 0), (ControlSetupControlGroupUI.ROW_HEIGHT, ControlSetupControlGroupUI.ROW_HEIGHT) ), sprite, controlUI.OnClick );

            UIText name = panel.AddText( new UILayoutInfo( UIFill.Horizontal( (1 - side) * ControlSetupControlGroupUI.ROW_HEIGHT, side * ControlSetupControlGroupUI.ROW_HEIGHT ), UIAnchor.Top, 0, ControlSetupControlGroupUI.ROW_HEIGHT ), attr.Name )
                .WithAlignment( side == 0 ? TMPro.HorizontalAlignmentOptions.Left : TMPro.HorizontalAlignmentOptions.Right );

            controlUI.Circle = button.rectTransform;
            controlUI.Side = side;

            return controlUI;
        }
    }
}