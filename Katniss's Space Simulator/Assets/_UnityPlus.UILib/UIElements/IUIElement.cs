using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public interface IUIElement
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
        /// Destroys the specified UI element along with its children UI elements.
        /// </summary>
        void Destroy();
    }
}