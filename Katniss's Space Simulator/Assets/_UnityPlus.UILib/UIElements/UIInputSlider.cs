using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIInputSlider : UIElement, IUIElementChild
    {
        // A slider produces float values, between a and b, with rounding to the nearest multiple of x.

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer Parent { get => _parent; }

        internal UIInputSlider( RectTransform transform, IUIElementContainer parent ) : base( transform )
        {
            this._parent = parent;
        }
    }
}