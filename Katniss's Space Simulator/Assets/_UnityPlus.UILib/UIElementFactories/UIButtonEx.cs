using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIButtonEx
    {
        public static UIButton AddButton( this UIElement parent, UILayoutInfo layout, Sprite sprite, UnityAction onClick )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-button", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.sprite = sprite;
            imageComponent.type = Image.Type.Sliced;

            Button buttonComponent = rootGameObject.AddComponent<Button>();
            buttonComponent.targetGraphic = imageComponent;
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
            buttonComponent.targetGraphic = imageComponent;

            if( onClick != null )
            {
                buttonComponent.onClick.AddListener( onClick ); // Find a way to cast System.Action to UnityAction if possible (the signatures of both delegates match).
            }

            return new UIButton( rootTransform, buttonComponent );
        }

        public static UIButton WithColors( this UIButton button, Color hovered, Color clicked, Color disabled )
        {
            button.buttonComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = hovered,
                pressedColor = clicked,
                disabledColor = disabled
            };

            return button;
        }
    }
}