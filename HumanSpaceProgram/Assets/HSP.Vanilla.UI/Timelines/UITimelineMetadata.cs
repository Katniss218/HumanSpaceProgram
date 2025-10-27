using HSP.Timelines.Serialization;
using HSP.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Timelines
{
    public class UITimelineMetadata : UIPanel, IPointerClickHandler
    {
        public ScenarioMetadata Scenario { get; private set; }
        public TimelineMetadata Timeline { get; private set; }

        public Action<UITimelineMetadata> onClick;

        public void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                onClick?.Invoke( this );
            }
        }

        public static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, ScenarioMetadata scenario, TimelineMetadata timeline, Action<UITimelineMetadata> onClick ) where T : UITimelineMetadata
        {
            T uiSaveMetadata = (T)UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/panel" ) )
                .Raycastable();

            uiSaveMetadata.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };
            uiSaveMetadata.Timeline = timeline;
            uiSaveMetadata.Scenario = scenario;
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
        public static UITimelineMetadata AddTimelineMetadata( this IUIElementContainer parent, UILayoutInfo layout, ScenarioMetadata scenario, TimelineMetadata timeline, Action<UITimelineMetadata> onClick )
        {
            return UITimelineMetadata.Create<UITimelineMetadata>( parent, layout, scenario, timeline, onClick );
        }
    }
}