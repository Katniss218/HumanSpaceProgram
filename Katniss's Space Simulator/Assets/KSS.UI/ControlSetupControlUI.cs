using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class ControlSetupControlUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform Circle { get; private set; }

        public ControlSetupControlGroupUI Group { get; private set; }
        public Control.Control Control { get; private set; }

        /// <summary>
        /// 0 or 1, depending on which side the endpoint is drawn on.
        /// </summary>
        public float Side { get; private set; }

        private NamedControlAttribute _attr;

        private static ControlSetupControlUI _startedConnection;

        void OnClick()
        {
            // on release on this, when dragging out a connection - connect.

            // show the name/description of channel on mouseover.
            // also when pressed if connected to something - disconnect and hook the end to the mouse until released. if released over nothing - delete connection

        }

        internal static ControlSetupControlUI Create( ControlSetupControlGroupUI group, float verticalOffset, Control.Control control, NamedControlAttribute attr )
        {
            UIPanel panel = group.panel.AddPanel( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, -verticalOffset, ControlSetupControlGroupUI.ROW_HEIGHT ), null );

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

            // UIButton button = panel.AddButton( new UILayoutInfo( (side, 1.0f), (0, 0), (ControlSetupControlGroupUI.ROW_HEIGHT, ControlSetupControlGroupUI.ROW_HEIGHT) ), sprite, controlUI.OnClick );

            UIIcon button = panel.AddIcon( new UILayoutInfo( (side, 1.0f), (0, 0), (ControlSetupControlGroupUI.ROW_HEIGHT, ControlSetupControlGroupUI.ROW_HEIGHT) ), sprite )
                .Raycastable(true);

            UIText name = panel.AddText( new UILayoutInfo( UIFill.Horizontal( (1 - side) * ControlSetupControlGroupUI.ROW_HEIGHT, side * ControlSetupControlGroupUI.ROW_HEIGHT ), UIAnchor.Top, 0, ControlSetupControlGroupUI.ROW_HEIGHT ), attr.Name )
                .WithAlignment( side == 0 ? TMPro.HorizontalAlignmentOptions.Left : TMPro.HorizontalAlignmentOptions.Right );

            ControlSetupControlUI controlUI = button.gameObject.AddComponent<ControlSetupControlUI>();
            controlUI.Group = group;
            controlUI.Control = control;
            controlUI._attr = attr;

            controlUI.Circle = button.rectTransform;
            controlUI.Side = side;

            return controlUI;
        }

        public void OnBeginDrag( PointerEventData eventData )
        {
            // Called when the drag *starts on this object*
            if( eventData.button != PointerEventData.InputButton.Left )
            {
                return;
            }

            _startedConnection = this;
        }

        public void OnDrag( PointerEventData eventData )
        {
            // Called if drag was started on this object.
            if( eventData.button != PointerEventData.InputButton.Left )
            {
                return;
            }

        }

        public void OnEndDrag( PointerEventData eventData )
        {
            // Called if drag that was started on this object ends. Regardless of where it ends.
            if( eventData.button != PointerEventData.InputButton.Left )
            {
                return;
            }

            ControlSetupControlUI targetControl = eventData.pointerCurrentRaycast.gameObject.GetComponent<ControlSetupControlUI>();
            if( targetControl == null )
            {
                Group.ComponentUI.Window.TryDisconnectWithMouse( this );
                return;
            }
            if( targetControl == this )
            {
                return;
            }

            Group.ComponentUI.Window.TryConnectWithMouse( this, targetControl );

            _startedConnection = null;
        }
    }
}