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

        internal IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        public Sprite Sprite { get => imageComponent.sprite; set => imageComponent.sprite = value; }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }

        public static UIIcon Create( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite icon )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-icon", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = icon;
            imageComponent.type = Image.Type.Simple;

            UIIcon iconC = rootGameObject.AddComponent<UIIcon>();
            iconC._parent = parent;
            iconC.Parent.Children.Add( iconC );
            iconC.imageComponent = imageComponent;
            return iconC;
        }
    }
}