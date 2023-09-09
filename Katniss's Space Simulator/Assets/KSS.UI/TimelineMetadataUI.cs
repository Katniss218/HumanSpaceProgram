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
        IUIElementContainer _saveContainer { get; set; }

        public TimelineMetadata Timeline { get; private set; }

        public override void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                foreach( var elem in _saveContainer.Children.ToArray() )
                {
                    elem.Destroy();
                }

                IEnumerable<SaveMetadata> saves = SaveMetadata.ReadAllSaves( Timeline.TimelineID );
                foreach( var save in saves )
                {
                    SaveMetadataUI.Create( _saveContainer, UILayoutInfo.FillHorizontal( 0, 0, 0f, 0, 200 ), save );
                }
            }

            base.OnPointerClick( eventData );
        }

        public static TimelineMetadataUI Create( IUIElementContainer parent, IUIElementContainer saveContainer, UILayoutInfo layout, TimelineMetadata timeline )
        {
            UIPanel panel = parent.AddPanel( layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) )
                .Raycastable();
            panel.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };

            TimelineMetadataUI component = panel.gameObject.AddComponent<TimelineMetadataUI>();
            component.Timeline = timeline;

            UIText t = panel.AddText( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0, 0.5f ), timeline.Name );
            t.FitToContents = true;
            t = panel.AddText( UILayoutInfo.FillHorizontal( 0, 0, 0f, 0.5f, 0f ), timeline.Description );
            t.FitToContents = true;

            component._saveContainer = saveContainer;

            UILayout.BroadcastLayoutUpdate( panel );

            return component;
        }
    }
}