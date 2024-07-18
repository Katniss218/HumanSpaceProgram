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
    public class UISaveMetadata : UIPanel, IPointerClickHandler
    {
        public SaveMetadata Save { get; private set; }

        public Action<UISaveMetadata> onClick;

        public void OnPointerClick( PointerEventData eventData )
        {
            if( eventData.button == PointerEventData.InputButton.Left )
            {
                onClick?.Invoke(this);
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, SaveMetadata save, Action<UISaveMetadata> onClick ) where T : UISaveMetadata
        {
            T uiSaveMetadata = (T)UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) )
                .Raycastable();

            uiSaveMetadata.LayoutDriver = new VerticalLayoutDriver() { FitToSize = true };
            uiSaveMetadata.Save = save;
            uiSaveMetadata.onClick = onClick;

            UIText nameText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0, 0.5f ), save.Name );

            nameText.FitToContents = true;

            UIText descriptionText = uiSaveMetadata.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Bottom, 0.5f, 0f ), save.Description );

            descriptionText.FitToContents = true;

            UILayoutManager.ForceLayoutUpdate( uiSaveMetadata );

            return uiSaveMetadata;
        }
    }

    public static class UISaveMetadata_Ex
    {
        public static UISaveMetadata AddSaveMetadata( this IUIElementContainer parent, UILayoutInfo layout, SaveMetadata save, Action<UISaveMetadata> onClick )
        {
            return UISaveMetadata.Create<UISaveMetadata>( parent, layout, save, onClick );
        }
    }
}