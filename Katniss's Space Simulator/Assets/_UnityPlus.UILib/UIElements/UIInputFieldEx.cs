using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputFieldEx
    {
        public static UIInputField AddInputField( this IUIElementContainer parent, UILayoutInfo layout, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-inputfield", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = true;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            (GameObject textareaGameObject, RectTransform textareaTransform) = UIElement.CreateUI( rootTransform, "uilib-inputfieldtextarea", new UILayoutInfo( Vector2.zero, Vector2.one, Vector2.zero, new Vector2( -10, -10 ) ) );

            RectMask2D mask = textareaGameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4( -5, -5, -5, -5 );

            (GameObject placeholderGameObject, _) = UIElement.CreateUI( textareaTransform, "uilib-inputfieldplaceholder", UILayoutInfo.Fill() );

            TMPro.TextMeshProUGUI placeholderText = placeholderGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            placeholderText.raycastTarget = false;
            placeholderText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            placeholderText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            placeholderText.fontStyle = TMPro.FontStyles.Italic;

            (GameObject textGameObject, _) = UIElement.CreateUI( textareaTransform, "uilib-inputfieldtext", UILayoutInfo.Fill() );

            TMPro.TextMeshProUGUI realText = textGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            realText.raycastTarget = false;
            realText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            realText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;

            TMPro.TMP_InputField inputFieldComponent = rootGameObject.AddComponent<TMPro.TMP_InputField>();
            inputFieldComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };

            inputFieldComponent.richText = false;
            inputFieldComponent.targetGraphic = imageComponent;
            inputFieldComponent.textViewport = textareaTransform;
            inputFieldComponent.textComponent = realText;
            inputFieldComponent.placeholder = placeholderText;
            inputFieldComponent.selectionColor = Color.gray;

            inputFieldComponent.RegenerateCaret();

            return new UIInputField( rootTransform, parent, inputFieldComponent, realText, placeholderText );
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