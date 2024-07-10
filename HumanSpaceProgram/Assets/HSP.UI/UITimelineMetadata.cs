using HSP.Core.Serialization;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.UI
{
    public class UITimelineMetadata : UIPanel, IPointerClickHandler
    {
        public TimelineMetadata Timeline { get; private set; }

        public Action<UITimelineMetadata> onClick;

        public void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                onClick?.Invoke( this );
            }
        }

        public static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, TimelineMetadata timeline, Action<UITimelineMetadata> onClick ) where T : UITimelineMetadata
        {
            T uiSaveMetadata = (T)UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) )
                .Raycastable();

            uiSaveMetadata.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };
            uiSaveMetadata.Timeline = timeline;
            uiSaveMetadata.onClick = onClick;

            UIText nameText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 0.5f ), timeline.Name );

            nameText.FitToContents = true;

            UIText descriptionText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0.5f, 0f ), timeline.Description );

            descriptionText.FitToContents = true;

            UILayoutManager.ForceLayoutUpdate( uiSaveMetadata );

            return uiSaveMetadata;
        }
    }

    public static class UITimelineMetadata_Ex
    {
        public static UITimelineMetadata AddTimelineMetadata( this IUIElementContainer parent, UILayoutInfo layout, TimelineMetadata timeline, Action<UITimelineMetadata> onClick )
        {
            return UITimelineMetadata.Create<UITimelineMetadata>( parent, layout, timeline, onClick );
        }
    }
}