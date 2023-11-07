using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIButtonEx
    {
        public static T AddButton<T>( this T parent, UILayoutInfo layout, Sprite sprite, Action onClick, out UIButton button ) where T : IUIElementContainer
        {
            button = UIButton.Create( parent, layout, sprite, onClick );
            return parent;
        }

        public static UIButton AddButton( this IUIElementContainer parent, UILayoutInfo layout, Sprite sprite, Action onClick )
        {
            return UIButton.Create( parent, layout, sprite, onClick );
        }

        public static UIButton WithTint( this UIButton button, Color tint )
        {
            var colors = button.buttonComponent.colors;
            button.buttonComponent.colors = new ColorBlock()
            {
                normalColor = tint,
                selectedColor = colors.selectedColor,
                colorMultiplier = colors.colorMultiplier,
                highlightedColor = colors.highlightedColor,
                pressedColor = colors.pressedColor,
                disabledColor = colors.disabledColor
            };

            return button;
        }

        public static UIButton WithColors( this UIButton button, Color hovered, Color clicked, Color disabled )
        {
            var colors = button.buttonComponent.colors;
            button.buttonComponent.colors = new ColorBlock()
            {
                normalColor = colors.normalColor,
                selectedColor = colors.selectedColor,
                colorMultiplier = colors.colorMultiplier,
                highlightedColor = hovered,
                pressedColor = clicked,
                disabledColor = disabled
            };

            return button;
        }

        public static UIButton Disabled( this UIButton button )
        {
            button.buttonComponent.interactable = false;

            return button;
        }
    }
}