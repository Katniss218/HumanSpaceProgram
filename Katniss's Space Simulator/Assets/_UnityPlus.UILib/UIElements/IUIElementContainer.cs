using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public interface IUILayoutDriven
    {
        LayoutDriver LayoutDriver { get; set; }
    }

    /// <summary>
    /// A UI element that can contain other UI elements.
    /// </summary>
    public interface IUIElementContainer
    {
        /// <summary>
        /// The 'root' transform of this UI element.
        /// </summary>
        RectTransform rectTransform { get; }

        /// <summary>
        /// The root GameObject of this UI element.
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// The immediate parent transform of the child elements.
        /// </summary>
        RectTransform contents { get; }

        /// <summary>
        /// The child elements of this UI element container.
        /// </summary>
        List<UIElement> Children { get; }
    }

    /// <summary>
    /// A UI element that can be contained by other UI elements.
    /// </summary>
    public interface IUIElementChild
    {
        /// <summary>
        /// The 'root' transform of this UI element.
        /// </summary>
        RectTransform rectTransform { get; }

        /// <summary>
        /// The root GameObject of this UI element.
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// The parent of this UI element.
        /// </summary>
        IUIElementContainer Parent { get; }
    }
}