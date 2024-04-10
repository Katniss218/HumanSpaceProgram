using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public partial class UIInputSlider : UIElement, IUIElementChild
    {
        // A slider produces float values, between a and b, with rounding to the nearest multiple of x.

        public IUIElementContainer Parent { get; set; }

        protected internal static T Create<T>() where T : UIInputSlider
        {
            throw new NotImplementedException();
        }
    }
}