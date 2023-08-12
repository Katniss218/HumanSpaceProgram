using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIIconEx
    {
        public static T WithIcon<T>( this T parent, UILayoutInfo layout, Sprite icon, out UIIcon uiIcon ) where T : IUIElementContainer
        {
            uiIcon = AddIcon( parent, layout, icon );
            return parent;
        }

        public static UIIcon AddIcon( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite icon )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-icon", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = icon;
            imageComponent.type = Image.Type.Simple;

            return new UIIcon( rootTransform, parent, imageComponent );
        }

        public static UIIcon WithTint( this UIIcon icon, Color tint )
        {
            icon.imageComponent.color = tint;
            return icon;
        }

        public static UIIcon Raycastable( this UIIcon icon, bool raycastable = true )
        {
            icon.imageComponent.raycastTarget = raycastable;

            return icon;
        }
    }
}