using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIContextMenu_Ex
    {
        public static UIContextMenu CreateContextMenu( this RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIContextMenu.Create<UIContextMenu>( track, contextMenuCanvas, layoutInfo, background );
        }

        public static UIContextMenu WithTint( this UIContextMenu contextMenu, Color tint )
        {
            contextMenu.backgroundComponent.color = tint;
            return contextMenu;
        }

        public static UIContextMenu Raycastable( this UIContextMenu contextMenu, bool raycastable = true )
        {
            contextMenu.backgroundComponent.raycastTarget = raycastable;

            return contextMenu;
        }
    }
}