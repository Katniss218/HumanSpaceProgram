using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIContextMenu : UIElement, IUIElementParent
    {
        internal readonly ContextMenu contextMenuComponent;
        internal readonly Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<UIElement> Children { get; }

        internal readonly IUIElementParent _parent;
        public IUIElementParent parent { get => _parent; }


        public UIContextMenu( RectTransform transform, IUIElementParent parent, ContextMenu contextMenuComponent, Image backgroundComponent ) : base( transform )
        {
            this._parent = parent;
            this.parent.Children.Add( this );
            this.contextMenuComponent = contextMenuComponent;
            this.backgroundComponent = backgroundComponent;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.parent.Children.Remove( this );
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }
    }
}