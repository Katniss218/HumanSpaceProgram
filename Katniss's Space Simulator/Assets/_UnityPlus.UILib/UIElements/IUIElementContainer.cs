using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that can contain other UI elements.
    /// </summary>
    public interface IUIElementContainer : IUIElement
    {
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