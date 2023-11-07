using System;
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

        public static UIInputToggle Create()
        {
            throw new NotImplementedException();
            // uiInputToggle._parent = parent;
        }
    }
}