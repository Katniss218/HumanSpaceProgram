using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIInputField : UIElement, IUIElementChild
    {
        internal TMPro.TMP_InputField inputFieldComponent;
        internal TMPro.TextMeshProUGUI textComponent;
        internal TMPro.TextMeshProUGUI placeholderComponent;
        internal Image backgroundComponent;

        public IUIElementContainer Parent { get; set; }

        public void SetOnTextChange( Action<string> onTextChange )
        {
            inputFieldComponent.onValueChanged.RemoveAllListeners();
            inputFieldComponent.onValueChanged.AddListener( ( s ) => onTextChange( inputFieldComponent.text ) );
        }

        public string Text { get => inputFieldComponent.text; set => inputFieldComponent.text = value; }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public static UIInputField Create( IUIElementContainer parent, UILayoutInfo layout, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform, UIInputField uiInputField) = UIElement.CreateUIGameObject<UIInputField>( parent, "uilib-inputfield", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = true;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            (GameObject textareaGameObject, RectTransform textareaTransform) = UIElement.CreateUIGameObject( rootTransform, "uilib-inputfieldtextarea", new UILayoutInfo( Vector2.zero, Vector2.one, Vector2.zero, new Vector2( -10, -10 ) ) );

            RectMask2D mask = textareaGameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4( -5, -5, -5, -5 );

            (GameObject placeholderGameObject, _) = UIElement.CreateUIGameObject( textareaTransform, "uilib-inputfieldplaceholder", UILayoutInfo.Fill() );

            TMPro.TextMeshProUGUI placeholderText = placeholderGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            placeholderText.raycastTarget = false;
            placeholderText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            placeholderText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            placeholderText.fontStyle = TMPro.FontStyles.Italic;

            (GameObject textGameObject, _) = UIElement.CreateUIGameObject( textareaTransform, "uilib-inputfieldtext", UILayoutInfo.Fill() );

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

            uiInputField.inputFieldComponent = inputFieldComponent;
            uiInputField.textComponent = realText;
            uiInputField.placeholderComponent = placeholderText;
            return uiInputField;
        }
    }
}