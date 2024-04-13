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
    public class ControlSetupControlUI : MonoBehaviour, IPointerEnterHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform Circle { get; private set; }

        public ControlSetupControlGroupUI Group { get; private set; }
        public Control.Control Control { get; private set; }

        /// <summary>
        /// 0 or 1, depending on which side the endpoint is drawn on.
        /// </summary>
        public float Side { get; private set; }

        public bool Editable { get; private set; }

        private NamedControlAttribute _attr;

        public void OnBeginDrag( PointerEventData eventData )
        {
            // Called when the drag *starts on this object*
            if( eventData.button != PointerEventData.InputButton.Left )
                return;

            if( !Editable )
                return;


        }

        public void OnDrag( PointerEventData eventData )
        {
            // Called if drag was started on this object.
            if( eventData.button != PointerEventData.InputButton.Left )
                return;

            if( !Editable )
                return;


        }

        public void OnEndDrag( PointerEventData eventData )
        {
            // Called if drag that was started on this object ends. Regardless of where it ends.
            if( eventData.button != PointerEventData.InputButton.Left )
                return;

            if( !Editable )
                return;

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
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            UITooltip contextMenu = ((RectTransform)this.transform).CreateTooltip( CanvasManager.Get( CanvasName.CURSOR ), new UILayoutInfo( UIAnchor.TopLeft, (0, 0), (250, 60) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/panel_tooltip" ) );

            contextMenu.AddText( new UILayoutInfo( UIFill.Horizontal( 5, 5 ), UIAnchor.Top, 0, 30 ), this._attr.Name )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 16, Color.white );
            contextMenu.AddText( new UILayoutInfo( UIFill.Horizontal( 5, 5 ), UIAnchor.Top, -30, 30 ), this._attr.Description )
                    .WithFont( AssetRegistry.Get<TMPro.TMP_FontAsset>( "builtin::Resources/Fonts/liberation_sans" ), 12, Color.white );
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

            UIIcon icon = panel.AddIcon( new UILayoutInfo( (side, 1.0f), (0, 0), (ControlSetupControlGroupUI.ROW_HEIGHT, ControlSetupControlGroupUI.ROW_HEIGHT) ), sprite )
                .Raycastable(true);

            UIText name = panel.AddText( new UILayoutInfo( UIFill.Horizontal( (1 - side) * ControlSetupControlGroupUI.ROW_HEIGHT, side * ControlSetupControlGroupUI.ROW_HEIGHT ), UIAnchor.Top, 0, ControlSetupControlGroupUI.ROW_HEIGHT ), attr.Name )
                .WithAlignment( side == 0 ? TMPro.HorizontalAlignmentOptions.Left : TMPro.HorizontalAlignmentOptions.Right );

            ControlSetupControlUI controlUI = icon.gameObject.AddComponent<ControlSetupControlUI>();
            controlUI.Group = group;
            controlUI.Control = control;
            controlUI._attr = attr;

            controlUI.Circle = icon.rectTransform;
            controlUI.Side = side;
            controlUI.Editable = attr.Editable;

            return controlUI;
        }
    }
}