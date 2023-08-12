using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIButton : UIElement, IUIElementContainer
    {
        internal readonly UnityEngine.UI.Button buttonComponent;
        internal readonly UnityEngine.UI.Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<UIElement> Children { get; }

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        internal UIButton( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.Button buttonComponent, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this._parent = parent;
            this.parent.Children.Add( this );
            Children = new List<UIElement>();
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