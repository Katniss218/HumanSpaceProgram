using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a simple icon UI element.
    /// </summary>
    public sealed class UIIcon : UIElement
    {
        internal readonly UnityEngine.UI.Image imageComponent;

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        internal UIIcon( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.Image imageComponent ) : base( transform )
        {
            this._parent = parent;
            this.parent.Children.Add( this );
            this.imageComponent = imageComponent;
        }

        public Sprite Sprite { get => imageComponent.sprite; set => imageComponent.sprite = value; }

        public override void Destroy()
        {
            base.Destroy();
            this.parent.Children.Remove( this );
        }
    }
}