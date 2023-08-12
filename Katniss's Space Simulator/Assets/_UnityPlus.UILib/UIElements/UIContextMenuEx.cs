using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIContextMenuEx
    {
        public static UIContextMenu CreateContextMenu( this RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( contextMenuCanvas.rectTransform, "uilib-contextmenu", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            ContextMenu contextMenu = rootGameObject.AddComponent<ContextMenu>();
            contextMenu.Target = track;

            return new UIContextMenu( track, contextMenuCanvas, contextMenu, backgroundComponent );
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