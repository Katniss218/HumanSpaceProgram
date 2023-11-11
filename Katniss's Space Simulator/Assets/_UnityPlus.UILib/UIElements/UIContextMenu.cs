using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A context menu contains a list of items.
    /// </summary>
    public sealed class UIContextMenu : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal ContextMenu contextMenuComponent;
        internal Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        void OnDestroy()
        {
            this.Parent.Children.Remove( this );
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public static UIContextMenu Create( RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUIGameObject( contextMenuCanvas.contents, "uilib-contextmenu", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            ContextMenu contextMenuComponent = rootGameObject.AddComponent<ContextMenu>();
            contextMenuComponent.Target = track;

            UIContextMenu uiContextMenu = rootGameObject.AddComponent<UIContextMenu>();
            //uiContextMenu.Parent = null;
            //uiContextMenu.Parent?.Children.Add( uiContextMenu );
            uiContextMenu.contextMenuComponent = contextMenuComponent;
            uiContextMenu.backgroundComponent = backgroundComponent;
            return uiContextMenu;
        }
    }
}