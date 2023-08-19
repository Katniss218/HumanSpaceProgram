using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a window, which is a defined section of the canvas.
    /// </summary>
    public sealed class UIWindow : UIElement, IUIElementContainer, IUILayoutDriven
    {
        internal readonly UnityEngine.UI.Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public UICanvas Parent { get; }
        public List<UIElement> Children { get; }
        public LayoutDriver LayoutDriver { get; set; }

        internal UIWindow( RectTransform transform, UICanvas parent, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this.Children = new List<UIElement>();
            this.Parent = parent;
            this.Parent.Children.Add( this );
            this.backgroundComponent = backgroundComponent;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }
    }
}