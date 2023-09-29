using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a simple icon UI element.
    /// </summary>
    public sealed class UIIcon : UIElement, IUIElementChild
    {
        internal readonly UnityEngine.UI.Image imageComponent;

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        internal UIIcon( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.Image imageComponent ) : base( transform )
        {
            this._parent = parent;
            this.Parent.Children.Add( this );
            this.imageComponent = imageComponent;
        }

        public Sprite Sprite { get => imageComponent.sprite; set => imageComponent.sprite = value; }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }
    }
}