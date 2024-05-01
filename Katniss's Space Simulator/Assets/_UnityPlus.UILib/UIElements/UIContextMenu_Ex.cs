using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIContextMenu_Ex
    {
        public static UIContextMenu CreateContextMenu( this IUIElementChild track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Action onDestroy = null )
        {
            return UIContextMenu.Create<UIContextMenu>( track.rectTransform, contextMenuCanvas, layoutInfo, background, onDestroy );
        }

        public static UIContextMenu CreateContextMenu( this RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Action onDestroy = null )
        {
            // This rectTransform direct overload is here for compatibility purposes.
            return UIContextMenu.Create<UIContextMenu>( track, contextMenuCanvas, layoutInfo, background, onDestroy );
        }
    }

    public partial class UIContextMenu
    {
        public UIContextMenu WithTint( Color tint )
        {
            this.backgroundComponent.color = tint;
            return this;
        }

        public UIContextMenu Raycastable( bool raycastable = true )
        {
            this.backgroundComponent.raycastTarget = raycastable;
            return this;
        }
    }
}