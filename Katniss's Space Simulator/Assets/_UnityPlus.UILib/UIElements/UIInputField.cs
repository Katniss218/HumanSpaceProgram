using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public partial class UIInputField : UIElement, IUIElementChild
    {
        protected TMPro.TMP_InputField inputFieldComponent;
        protected TMPro.TextMeshProUGUI textComponent;
        protected TMPro.TextMeshProUGUI placeholderComponent;
        protected Image backgroundComponent;

        public IUIElementContainer Parent { get; set; }

        public void SetOnTextChange( Action<string> onTextChange )
        {
            inputFieldComponent.onValueChanged.RemoveAllListeners();
            inputFieldComponent.onValueChanged.AddListener( ( s ) => onTextChange( inputFieldComponent.text ) );
        }

        public virtual string Text { get => inputFieldComponent.text; set => inputFieldComponent.text = value; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, Sprite background ) where T : UIInputField
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiInputField) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{nameof( T )}", layout );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = true;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Sliced;

            (GameObject textareaGameObject, RectTransform textareaTransform) = UIElement.CreateUIGameObject( rootTransform, $"uilib-{nameof( T )}-textarea", new UILayoutInfo( UIFill.Fill( 5, 5, 5, 5 ) ) );

            RectMask2D mask = textareaGameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4( -5, -5, -5, -5 );

            (GameObject placeholderGameObject, _) = UIElement.CreateUIGameObject( textareaTransform, $"uilib-{nameof( T )}-placeholder", new UILayoutInfo( UIFill.Fill() ) );

            TMPro.TextMeshProUGUI placeholderText = placeholderGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            placeholderText.raycastTarget = false;
            placeholderText.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
            placeholderText.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            placeholderText.fontStyle = TMPro.FontStyles.Italic;

            (GameObject textGameObject, _) = UIElement.CreateUIGameObject( textareaTransform, $"uilib-{nameof( T )}-text", new UILayoutInfo( UIFill.Fill() ) );

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