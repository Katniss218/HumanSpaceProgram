using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a window, which is a defined section of the canvas.
    /// </summary>
    public sealed class UIWindow : UIElement, IUIElementContainer
    {
        internal readonly UnityEngine.UI.Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<UIElement> Children { get; }
        internal readonly IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        internal UIWindow( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this._parent = parent;
            this.parent.Children.Add( this );
            Children = new List<UIElement>();
            this.backgroundComponent = backgroundComponent;
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public override void Destroy()
        {
            base.Destroy();
            this.parent.Children.Remove( this );
        }
    }
}