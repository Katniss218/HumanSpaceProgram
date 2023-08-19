using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that can contain other UI elements.
    /// </summary>
    public interface IUIElementContainer
    {
        /// <summary>
        /// The root GameObject of this UI element.
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// The 'root' transform of this UI element.
        /// </summary>
        RectTransform rectTransform { get; }

        /// <summary>
        /// The immediate parent transform of the child elements.
        /// </summary>
        RectTransform contents { get; }

        /// <summary>
        /// The child elements of this UI element container.
        /// </summary>
        List<IUIElementChild> Children { get; }
    }
}