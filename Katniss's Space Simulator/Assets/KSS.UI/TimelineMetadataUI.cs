using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class TimelineMetadataUI : EventTrigger
    {
        public TimelineMetadata Timeline { get; private set; }

        public Action<TimelineMetadataUI> onClick;

        public override void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                onClick?.Invoke( this );
            }

            base.OnPointerClick( eventData );
        }

        public static TimelineMetadataUI Create( IUIElementContainer parent, UILayoutInfo layout, TimelineMetadata timeline, Action<TimelineMetadataUI> onClick )
        {
            UIPanel panel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) )
                .Raycastable();
            panel.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            TimelineMetadataUI component = panel.gameObject.AddComponent<TimelineMetadataUI>();
            component.Timeline = timeline;
            component.onClick = onClick;

            UIText t = panel.AddText( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0, 0.5f ), timeline.Name );
            t.FitToContents = true;
            t = panel.AddText( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0.5f, 0f ), timeline.Description );
            t.FitToContents = true;

            UILayout.BroadcastLayoutUpdate( panel );

            return component;
        }
    }
}