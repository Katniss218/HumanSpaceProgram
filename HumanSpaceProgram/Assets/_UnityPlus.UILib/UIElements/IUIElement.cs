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
        Transform transform { get; }

        GameObject gameObject { get; }

        /// <summary>
        /// The 'root' transform of this UI element.
        /// </summary>
        RectTransform rectTransform { get; }

        void Destroy();
    }
}