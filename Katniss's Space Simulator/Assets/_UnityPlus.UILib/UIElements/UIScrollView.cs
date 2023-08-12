using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIScrollView : UIElement, IUIElementContainer
    {
        internal readonly UnityEngine.UI.ScrollRect scrollRectComponent;

        public UIScrollBar scrollbarHorizontal;
        public UIScrollBar scrollbarVertical;
        readonly RectTransform _contents;
        public RectTransform contents { get => _contents; }

        public List<UIElement> Children { get; }

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        internal UIScrollView( RectTransform transform, IUIElementContainer parent, UnityEngine.UI.ScrollRect scrollRectComponent, UIScrollBar scrollbarHorizontal, UIScrollBar scrollbarVertical, RectTransform contents ) : base( transform )
        {
            this._parent = parent;
            this.parent.Children.Add( this );
            Children = new List<UIElement>();
            this.scrollRectComponent = scrollRectComponent;
            this.scrollbarHorizontal = scrollbarHorizontal;
            this.scrollbarVertical = scrollbarVertical;
            this._contents = contents;
        }

        public override void Destroy()
        {
            base.Destroy();
            this.parent.Children.Remove( this );
        }
    }
}