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
        /// The <see cref="RectTransform"/> whoose children are the children of this container.
        /// </summary>
        RectTransform contents { get; }

        /// <summary>
        /// The child elements of this container.
        /// </summary>
        List<IUIElementChild> Children { get; }
    }
}