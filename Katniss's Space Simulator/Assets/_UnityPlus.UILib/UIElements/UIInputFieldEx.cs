using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputFieldEx
    {
        public static UIInputField AddInputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField.Create( parent, layoutInfo, background );
        }

        public static UIInputField WithMargins( this UIInputField inputField, float left, float right, float top, float bottom )
        {
            inputField.textComponent.rectTransform.SetLayoutInfo( UILayoutInfo.Fill( left, right, top, bottom ) );
            inputField.placeholderComponent.rectTransform.SetLayoutInfo( UILayoutInfo.Fill( left, right, top, bottom ) );
            return inputField;
        }

        public static UIInputField WithFont( this UIInputField inputField, TMPro.TMP_FontAsset font, float fontSize, Color color )
        {
            var textComponent = inputField.textComponent;
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = color;

            var placeholderComponent = inputField.placeholderComponent;
            placeholderComponent.font = font;
            placeholderComponent.fontSize = fontSize;
            placeholderComponent.color = new Color( color.r, color.g, color.b, color.a * 0.5f );

            return inputField;
        }

        public static UIInputField WithPlaceholder( this UIInputField inputField, string placeholderText )
        {
            inputField.placeholderComponent.text = placeholderText;
            return inputField;
        }
    }

    public static class TMP_InputField_Ex
    {
        public static void RegenerateCaret( this TMPro.TMP_InputField inputField )
        {
            inputField.enabled = false;
            inputField.enabled = true; // regenerate the caret. For some reason this works... :shrug:
        }
    }
}