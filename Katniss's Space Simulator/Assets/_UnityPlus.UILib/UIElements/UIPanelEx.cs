using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIPanelEx
    {
        public static T WithPanel<T>( this T parent, UILayoutInfo layout, Sprite background, out UIPanel uiPanel ) where T : IUIElementParent
        {
            uiPanel = AddPanel( parent, layout, background );
            return parent;
        }

        public static UIPanel AddPanel( this IUIElementParent parent, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-panel", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            return new UIPanel( rootTransform, parent, backgroundComponent );
        }

        public static UIPanel WithTint( this UIPanel panel, Color tint )
        {
            panel.backgroundComponent.color = tint;
            return panel;
        }

        public static UIPanel Raycastable( this UIPanel panel, bool raycastable = true )
        {
            panel.backgroundComponent.raycastTarget = raycastable;

            return panel;
        }
    }
}