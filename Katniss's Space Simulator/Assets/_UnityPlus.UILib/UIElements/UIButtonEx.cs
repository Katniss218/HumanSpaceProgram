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
        public static T AddButton<T>( this T parent, UILayoutInfo layout, Sprite sprite, UnityAction onClick, out UIButton button ) where T : IUIElementParent
        {
            button = AddButton( parent, layout, sprite, onClick );
            return parent;
        }

        public static UIButton AddButton( this IUIElementParent parent, UILayoutInfo layout, Sprite sprite, UnityAction onClick = null )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-button", layout );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = sprite;
            backgroundComponent.type = Image.Type.Sliced;

            Button buttonComponent = rootGameObject.AddComponent<Button>();
            buttonComponent.targetGraphic = backgroundComponent;
            buttonComponent.transition = Selectable.Transition.ColorTint;
            buttonComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };
            buttonComponent.targetGraphic = backgroundComponent;

            if( onClick != null )
            {
                buttonComponent.onClick.AddListener( onClick ); // Find a way to cast System.Action to UnityAction if possible (the signatures of both delegates match).
            }

            return new UIButton( rootTransform, parent, buttonComponent, backgroundComponent );
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