using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a simple icon UI element.
    /// </summary>
    public sealed class UIIcon : UIElement, IUIElementChild
    {
        internal Image imageComponent;

        public IUIElementContainer Parent { get; set; }

        public LayoutDriver LayoutDriver { get; set; }

        public Sprite Sprite { get => imageComponent.sprite; set => imageComponent.sprite = value; }

        public static UIIcon Create( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite icon )
        {
            (GameObject rootGameObject, RectTransform rootTransform, UIIcon uiIcon) = UIElement.CreateUIGameObject<UIIcon>( parent, "uilib-icon", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = icon;
            imageComponent.type = Image.Type.Simple;

            uiIcon.imageComponent = imageComponent;
            return uiIcon;
        }
    }
}