using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public interface IUICustomLayout
    {
        /// <summary>
        /// The current layout driver driving the positioning of child elements. Can be null for manual control.
        /// </summary>
        LayoutDriver LayoutDriver { get; }
    }

    /// <summary>
    /// A UI element that can contain other UI elements.
    /// </summary>
    public interface IUIElementContainer
    {
        RectTransform rectTransform { get; }
        RectTransform contents { get; }
        GameObject gameObject { get; }

        List<UIElement> Children { get; }

    }

    /// <summary>
    /// A UI element that can be contained by other UI elements.
    /// </summary>
    public interface IUIElementChild
    {
        RectTransform rectTransform { get; }
        GameObject gameObject { get; }

        IUIElementContainer Parent { get; }
    }
}