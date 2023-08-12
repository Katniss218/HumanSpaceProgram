using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIScrollView : UIElement, IUIElementContainer, IUIElementChild
    {
        internal readonly UnityEngine.UI.ScrollRect scrollRectComponent;

        public UIScrollBar scrollbarHorizontal;
        public UIScrollBar scrollbarVertical;
        readonly RectTransform _contents;
        public RectTransform contents { get => _contents; }

        public List<UIElement> Children { get; }

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        public LayoutDriver LayoutDriver { get; set; }

        internal UIScrollView( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.ScrollRect scrollRectComponent, UIScrollBar scrollbarHorizontal, UIScrollBar scrollbarVertical, RectTransform contents ) : base( transform )
        {
            Children = new List<UIElement>();
            this._parent = parent;
            this.Parent.Children.Add( this );
            this.scrollRectComponent = scrollRectComponent;
            this.scrollbarHorizontal = scrollbarHorizontal;
            this.scrollbarVertical = scrollbarVertical;
            this._contents = contents;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.Parent.Children.Remove( this );
        }
    }
}