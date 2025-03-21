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
    public class UIScenarioMetadata : UIPanel, IPointerClickHandler
    {
        public ScenarioMetadata Scenario { get; private set; }

        public Action<UIScenarioMetadata> onClick;

        public void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                onClick?.Invoke( this );
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, ScenarioMetadata scenario, Action<UIScenarioMetadata> onClick ) where T : UIScenarioMetadata
        {
            T uiSaveMetadata = (T)UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) )
                .Raycastable();

            uiSaveMetadata.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };
            uiSaveMetadata.Scenario = scenario;
            uiSaveMetadata.onClick = onClick;

            UIIcon iconUI = uiSaveMetadata.AddIcon( new UILayoutInfo( UIAnchor.Left, (0, 0), (100, 100) ), scenario.Icon );

            UIText nameText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal( 100, 0 ), UIAnchor.Bottom, 0, 0.5f ), scenario.Name );

            nameText.FitToContents = true;

            UIText descriptionText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal( 100, 0 ), UIAnchor.Bottom, 0.5f, 0f ), scenario.Description );

            descriptionText.FitToContents = true;

            UILayoutManager.ForceLayoutUpdate( uiSaveMetadata );

            return uiSaveMetadata;
        }
    }

    public static class UIScenarioMetadata_Ex
    {
        public static UIScenarioMetadata AddSaveMetadata( this IUIElementContainer parent, UILayoutInfo layout, ScenarioMetadata scenario, Action<UIScenarioMetadata> onClick )
        {
            return UIScenarioMetadata.Create<UIScenarioMetadata>( parent, layout, scenario, onClick );
        }
    }
}