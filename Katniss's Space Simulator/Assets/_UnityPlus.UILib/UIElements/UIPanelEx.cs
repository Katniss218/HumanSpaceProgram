using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIPanelEx
    {
        public static T WithPanel<T>( this T parent, UILayoutInfo layoutInfo, Sprite background, out UIPanel uiPanel ) where T : IUIElementContainer
        {
            uiPanel = UIPanel.Create( parent, layoutInfo, background );
            return parent;
        }

        public static UIPanel AddPanel( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIPanel.Create( parent, layoutInfo, background );
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