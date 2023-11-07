using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIIconEx
    {
        public static T WithIcon<T>( this T parent, UILayoutInfo layoutInfo, Sprite icon, out UIIcon uiIcon ) where T : IUIElementContainer
        {
            uiIcon = UIIcon.Create( parent, layoutInfo, icon );
            return parent;
        }

        public static UIIcon AddIcon( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite icon )
        {
            return UIIcon.Create( parent, layoutInfo, icon );
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