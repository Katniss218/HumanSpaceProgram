using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{

    /// <summary>
    /// A UI element that can be contained by other UI elements.
    /// </summary>
    public interface IUIElementChild
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
        /// The parent of this UI element.
        /// </summary>
        IUIElementContainer Parent { get; }
    }
}