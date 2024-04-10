using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputField_Ex
    {
        public static T WithInputField<T>( this T parent, UILayoutInfo layoutInfo, Sprite background, out UIInputField uiInputField ) where T : IUIElementContainer
        {
            uiInputField = UIInputField.Create<UIInputField>( parent, layoutInfo, background );
            return parent;
        }

        public static UIInputField AddInputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField.Create<UIInputField>( parent, layoutInfo, background );
        }
    }

    public partial class UIInputField
    {
        public UIInputField WithMargins( float left, float right, float top, float bottom )
        {
            this.textComponent.rectTransform.SetLayoutInfo( new UILayoutInfo( UIFill.Fill( left, right, top, bottom ) ) );
            this.placeholderComponent.rectTransform.SetLayoutInfo( new UILayoutInfo( UIFill.Fill( left, right, top, bottom ) ) );
            return this;
        }

        public UIInputField WithFont( TMPro.TMP_FontAsset font, float fontSize, Color color )
        {
            var textComponent = this.textComponent;
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = color;

            var placeholderComponent = this.placeholderComponent;
            placeholderComponent.font = font;
            placeholderComponent.fontSize = fontSize;
            placeholderComponent.color = new Color( color.r, color.g, color.b, color.a * 0.5f );

            return this;
        }

        public UIInputField WithPlaceholder( string placeholderText )
        {
            this.placeholderComponent.text = placeholderText;
            return this;
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