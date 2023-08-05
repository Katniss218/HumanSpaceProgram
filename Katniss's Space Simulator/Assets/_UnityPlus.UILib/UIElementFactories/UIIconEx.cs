using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIIconEx
    {
        public static T WithIcon<T>( this T parent, UILayoutInfo layout, Sprite icon, out UIIcon uiIcon ) where T : UIElement
        {
            uiIcon = AddIcon( parent, layout, icon );
            return parent;
        }

        public static UIIcon AddIcon( this UIElement parent, UILayoutInfo layoutInfo, Sprite icon )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-icon", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = icon;
            imageComponent.type = Image.Type.Simple;

            return new UIIcon( rootTransform, imageComponent );
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