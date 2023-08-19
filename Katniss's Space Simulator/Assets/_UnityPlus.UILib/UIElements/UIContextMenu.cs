using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A context menu contains a list of items.
    /// </summary>
    public sealed class UIContextMenu : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal readonly ContextMenu contextMenuComponent;
        internal readonly Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public List<IUIElementChild> Children { get; }

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }


        public UIContextMenu( RectTransform transform, IUIElementContainer parent, ContextMenu contextMenuComponent, Image backgroundComponent ) : base( transform )
        {
            this.Children = new List<IUIElementChild>();
            this._parent = parent;
            this.Parent.Children.Add( this );
            this.contextMenuComponent = contextMenuComponent;
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