using HSP.Timelines.Serialization;
using HSP.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
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
            T uiSaveMetadata = (T)UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/panel" ) )
                .Raycastable();

            uiSaveMetadata.Scenario = scenario;
            uiSaveMetadata.onClick = onClick;

            UIIcon iconUI = uiSaveMetadata.AddIcon( new UILayoutInfo( UIAnchor.Left, (0, 0), (100, 100) ), scenario.Icon );

            UIText nameText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal( 110, 10 ), UIAnchor.Top, 0, 30f ), scenario.Name );

            UIText descriptionText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Fill( 110, 10, 30, 0 ) ), scenario.Description )
                .WithAlignment( TMPro.VerticalAlignmentOptions.Top );

            UILayoutManager.ForceLayoutUpdate( uiSaveMetadata );

            return uiSaveMetadata;
        }
    }

    public static class UIScenarioMetadata_Ex
    {
        public static UIScenarioMetadata AddScenarioMetadata( this IUIElementContainer parent, UILayoutInfo layout, ScenarioMetadata scenario, Action<UIScenarioMetadata> onClick )
        {
            return UIScenarioMetadata.Create<UIScenarioMetadata>( parent, layout, scenario, onClick );
        }
    }
}