using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIInputSlider : UIElement
    {
        // A slider produces float values, between a and b, with rounding to the nearest multiple of x.

        internal readonly IUIElementParent _parent;
        public IUIElementParent parent { get => _parent; }

        internal UIInputSlider( RectTransform transform, IUIElementParent parent ) : base( transform )
        {
            this._parent = parent;
        }
    }
}