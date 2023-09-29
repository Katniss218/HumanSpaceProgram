using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a section of the canvas, or of a different UI element.
    /// </summary>
    public sealed class UIPanel : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal readonly UnityEngine.UI.Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<IUIElementChild> Children { get; }
        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        internal UIPanel( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            Children = new List<IUIElementChild>();
            this._parent = parent;
            this.Parent.Children.Add( this );
            this.backgroundComponent = backgroundComponent;
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }
    }
}