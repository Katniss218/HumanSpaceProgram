using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIButton : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal readonly UnityEngine.UI.Button buttonComponent;
        internal readonly UnityEngine.UI.Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<UIElement> Children { get; }

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        internal UIButton( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.Button buttonComponent, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            Children = new List<UIElement>();
            this._parent = parent;
            this.Parent.Children.Add( this );
            this.buttonComponent = buttonComponent;
            this.backgroundComponent = backgroundComponent;
        }

        public override void Destroy()
        {
            base.Destroy();
            _parent.Children.Remove( this );
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public UnityEvent onClick => buttonComponent.onClick;
    }
}