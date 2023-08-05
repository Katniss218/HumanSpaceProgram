using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIInputFieldEx
    {
        public static UIInputField AddInputField( this UIElement parent, UILayoutInfo layout, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-inputfield", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

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

            (GameObject textareaGameObject, RectTransform textareaTransform) = UIHelper.CreateUI( rootTransform, "uilib-inputfieldtextarea", new UILayoutInfo( Vector2.zero, Vector2.one, Vector2.zero, new Vector2( -10, -10 ) ) );

            RectMask2D mask = textareaGameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4( -5, -5, -5, -5 );

            (GameObject placeholderGameObject, _) = UIHelper.CreateUI( textareaTransform, "uilib-inputfieldplaceholder", UILayoutInfo.Fill() );

            TMPro.TextMeshProUGUI placeholderText = placeholderGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            placeholderText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            placeholderText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            placeholderText.fontStyle = TMPro.FontStyles.Italic;

            (GameObject textGameObject, _) = UIHelper.CreateUI( textareaTransform, "uilib-inputfieldtext", UILayoutInfo.Fill() );

            TMPro.TextMeshProUGUI realText = textGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            realText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            realText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;

            inputFieldComponent.targetGraphic = imageComponent;
            inputFieldComponent.textViewport = textareaTransform;
            inputFieldComponent.textComponent = realText;
            inputFieldComponent.placeholder = placeholderText;
            inputFieldComponent.selectionColor = Color.gray;

            return new UIInputField( rootTransform, inputFieldComponent, realText, placeholderText );
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
    }
}