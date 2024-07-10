using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIPanel_Ex
    {
        public static T WithPanel<T>( this T parent, UILayoutInfo layoutInfo, Sprite background, out UIPanel uiPanel ) where T : IUIElementContainer
        {
            uiPanel = UIPanel.Create<UIPanel>( parent, layoutInfo, background );
            return parent;
        }

        public static UIPanel AddPanel( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIPanel.Create<UIPanel>( parent, layoutInfo, background );
        }
    }

    public partial class UIPanel
    {
        public UIPanel WithTint( Color tint )
        {
            this.backgroundComponent.color = tint;
            return this;
        }

        public UIPanel Raycastable( bool raycastable = true )
        {
            this.backgroundComponent.raycastTarget = raycastable;

            return this;
        }
    }
}