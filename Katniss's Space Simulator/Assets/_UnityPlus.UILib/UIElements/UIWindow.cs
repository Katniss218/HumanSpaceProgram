using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a window, which is a defined section of the canvas.
    /// </summary>
    public sealed class UIWindow : UIElement, IUIElementContainer, IUIElementChild /* The window really shouldn't be a child tbh. It can only be the child of a canvas. */, IUILayoutDriven
    {
        internal Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public static UIWindow Create( UICanvas parent, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform, UIWindow uiWindow) = UIElement.CreateUIGameObject<UIWindow>( parent, "uilib-window", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            uiWindow.backgroundComponent = backgroundComponent;
            return uiWindow;
        }
    }
}