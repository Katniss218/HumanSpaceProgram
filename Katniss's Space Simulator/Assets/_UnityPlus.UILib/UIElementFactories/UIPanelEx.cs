using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIPanelEx
    {
        public static UIPanel AddPanel( this Canvas parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return AddPanel( (UIElement)parent.transform, layoutInfo, background );
        }

        public static T WithPanel<T>( this T parent, UILayoutInfo layout, Sprite background, out UIPanel uiPanel ) where T : UIElement
        {
            uiPanel = AddPanel( parent, layout, background );
            return parent;
        }

        public static UIPanel AddPanel( this UIElement parent, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-panel", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            return new UIPanel( rootTransform, imageComponent );
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