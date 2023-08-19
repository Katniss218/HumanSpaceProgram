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
        /// Checks whether or not the specified UI element has been permanently destroyed (not visible).
        /// </summary>
        public static bool IsDestroyed( this UIElement uiElement )
        {
            return uiElement == null || uiElement.IsDestroyed;
        }
    }
}