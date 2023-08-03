using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UILib.Factories
{
    public static class ButtonFactory
    {
        public static (RectTransform root, Button) CreateTextXY( RectTransform parent, string name, string text, UILayoutInfo layoutInfo, UIStyle style )
        {
            (GameObject rootGO, RectTransform rootT) = UIHelper.CreateUI( parent, name, layoutInfo );

            Image image = rootGO.AddComponent<Image>();
            image.sprite = style.Button2Axis;
            image.type = Image.Type.Sliced;

            Button button = rootGO.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = style.ButtonHover,
                pressedColor = style.ButtonClick,
                disabledColor = style.ButtonDisabled
            };

            TextFactory.CreateText( rootT, "text", text, TMPro.HorizontalAlignmentOptions.Center, TMPro.VerticalAlignmentOptions.Middle, UILayoutInfo.Fill(), style );

            return (rootT, button);
        }

#warning TODO - custom icon buttons might be defined by a different entry in a uistyle. this could I guess be solved by having a factory specializing in making an arbitrary button, or a translation layer, or something else.
    }
}