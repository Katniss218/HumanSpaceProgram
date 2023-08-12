using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A UI element that can contain other UI elements.
    /// </summary>
    public interface IUIElementContainer
    {
        RectTransform contents { get; }
        GameObject gameObject { get; }

        List<UIElement> Children { get; }

        //void OnLayoutUpdate( UIElement updatedDirectChildElement, Vector2 sizeDelta );
    }
}
