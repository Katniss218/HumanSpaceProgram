using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputField_Ex
    {
        public static T WithInputField<T, TValue>( this T parent, UILayoutInfo layoutInfo, Sprite background, out UIInputField<TValue> uiInputField
            , Func<string, bool> validator, Func<string, TValue> stringToValue, Func<TValue, string> valueToString ) where T : IUIElementContainer
        {
            uiInputField = UIInputField<TValue>.Create<UIInputField<TValue>>( parent, layoutInfo, background, validator, stringToValue, valueToString );
            return parent;
        }

        public static UIInputField<TValue> AddInputField<TValue>( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background
            , Func<string, bool> validator, Func<string, TValue> stringToValue, Func<TValue, string> valueToString )
        {
            return UIInputField<TValue>.Create<UIInputField<TValue>>( parent, layoutInfo, background, validator, stringToValue, valueToString );
        }


        public static UIInputField<int> AddInt32InputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField<int>.Create<UIInputField<int>>( parent, layoutInfo, background, s => int.TryParse( s, out _ ), s => int.Parse( s ), v => v.ToString( "D" ) );
        }
        
        public static UIInputField<long> AddInt64InputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField<long>.Create<UIInputField<long>>( parent, layoutInfo, background, s => long.TryParse( s, out _ ), s => long.Parse( s ), v => v.ToString( "D" ) );
        }

        public static UIInputField<float> AddFloatInputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField<float>.Create<UIInputField<float>>( parent, layoutInfo, background, s => float.TryParse( s, out _ ), s => float.Parse( s ), v => v.ToString() );
        }

        public static UIInputField<double> AddDoubleInputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField<double>.Create<UIInputField<double>>( parent, layoutInfo, background, s => double.TryParse( s, out _ ), s => double.Parse( s ), v => v.ToString() );
        }

        public static UIInputField<string> AddStringInputField( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIInputField<string>.Create<UIInputField<string>>( parent, layoutInfo, background, s => true, s => s, v => v );
        }
    }

    public partial class UIInputField<TValue>
    {
        public UIInputField<TValue> WithMargins( float left, float right, float top, float bottom )
        {
            this.textComponent.rectTransform.SetLayoutInfo( new UILayoutInfo( UIFill.Fill( left, right, top, bottom ) ) );
            this.placeholderComponent.rectTransform.SetLayoutInfo( new UILayoutInfo( UIFill.Fill( left, right, top, bottom ) ) );
            return this;
        }

        public UIInputField<TValue> WithFont( TMPro.TMP_FontAsset font, float fontSize, Color fontColor )
        {
            var textComponent = this.textComponent;
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = fontColor;

            var placeholderComponent = this.placeholderComponent;
            placeholderComponent.font = font;
            placeholderComponent.fontSize = fontSize;
            placeholderComponent.color = new Color( fontColor.r, fontColor.g, fontColor.b, fontColor.a * 0.5f );

            return this;
        }

        public UIInputField<TValue> WithPlaceholder( string placeholderText )
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