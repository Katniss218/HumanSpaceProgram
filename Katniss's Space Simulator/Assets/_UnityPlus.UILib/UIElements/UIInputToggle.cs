using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIInputToggle : UIElement
    {
        // Toggle has 2 sprites, one for inactive, and other for active.

        internal readonly IUIElementContainer _parent;
        public IUIElementContainer parent { get => _parent; }

        internal UIInputToggle( RectTransform transform, IUIElementContainer parent ) : base( transform )
        {
            this._parent = parent;
        }
    }
}