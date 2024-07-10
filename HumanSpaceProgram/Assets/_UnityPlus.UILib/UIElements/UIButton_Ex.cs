using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIButton_Ex
    {
        public static T WithButton<T>( this T parent, UILayoutInfo layout, Sprite sprite, Action onClick, out UIButton button ) where T : IUIElementContainer
        {
            button = UIButton.Create<UIButton>( parent, layout, sprite, onClick );
            return parent;
        }

        public static UIButton AddButton( this IUIElementContainer parent, UILayoutInfo layout, Sprite sprite, Action onClick )
        {
            return UIButton.Create<UIButton>( parent, layout, sprite, onClick );
        }
    }

    public partial class UIButton
    {
        public UIButton WithTint( Color tint )
        {
            var colors = this.buttonComponent.colors;
            this.buttonComponent.colors = new ColorBlock()
            {
                normalColor = tint,
                selectedColor = colors.selectedColor,
                colorMultiplier = colors.colorMultiplier,
                highlightedColor = colors.highlightedColor,
                pressedColor = colors.pressedColor,
                disabledColor = colors.disabledColor
            };

            return this;
        }

        public UIButton WithColors( Color hovered, Color clicked, Color disabled )
        {
            var colors = this.buttonComponent.colors;
            this.buttonComponent.colors = new ColorBlock()
            {
                normalColor = colors.normalColor,
                selectedColor = colors.selectedColor,
                colorMultiplier = colors.colorMultiplier,
                highlightedColor = hovered,
                pressedColor = clicked,
                disabledColor = disabled
            };

            return this;
        }

        public UIButton Disabled()
        {
            this.buttonComponent.interactable = false;

            return this;
        }
    }
}