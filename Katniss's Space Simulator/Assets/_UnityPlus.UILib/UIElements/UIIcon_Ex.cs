using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIIcon_Ex
    {
        public static T WithIcon<T>( this T parent, UILayoutInfo layoutInfo, Sprite icon, out UIIcon uiIcon ) where T : IUIElementContainer
        {
            uiIcon = UIIcon.Create<UIIcon>( parent, layoutInfo, icon );
            return parent;
        }

        public static UIIcon AddIcon( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite icon )
        {
            return UIIcon.Create<UIIcon>( parent, layoutInfo, icon );
        }
    }

    public partial class UIIcon 
    {
        public UIIcon WithTint( Color tint )
        {
            this.imageComponent.color = tint;
            return this;
        }

        public UIIcon Raycastable( bool raycastable = true )
        {
            this.imageComponent.raycastTarget = raycastable;

            return this;
        }
    }
}