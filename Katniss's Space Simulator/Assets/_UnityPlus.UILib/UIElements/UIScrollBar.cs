using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIScrollBar : UIElement
    {
        internal readonly UnityEngine.UI.Scrollbar scrollbarComponent;

        internal readonly IUIElementParent _parent;
        public IUIElementParent parent { get => _parent; }

        internal UIScrollBar( RectTransform transform, IUIElementParent parent, UnityEngine.UI.Scrollbar scrollbarComponent ) : base( transform )
        {
            this._parent = parent;
            this.scrollbarComponent = scrollbarComponent;
        }
    }
}