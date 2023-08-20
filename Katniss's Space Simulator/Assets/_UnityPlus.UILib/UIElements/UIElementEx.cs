using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public static class UIElementEx
    {
        /// <summary>
        /// Checks whether or not the specified UI element has been destroyed.
        /// </summary>
        public static bool IsNullOrDestroyed( this IUIElement uiElement )
        {
            return uiElement == null || uiElement.gameObject == null;
        }
    }
}