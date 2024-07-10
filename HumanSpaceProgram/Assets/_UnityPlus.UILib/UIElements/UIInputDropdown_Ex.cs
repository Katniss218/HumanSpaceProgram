using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIInputDropdown_Ex
    {
        public static T WithInputDropdown<T, TValue>( this T parent, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Sprite backgroundActive, Sprite contextMenuBackground, Sprite elementBackground, Func<TValue, string> valueToString, out UIInputDropdown<TValue> uiInputField ) where T : IUIElementContainer
        {
            uiInputField = UIInputDropdown<TValue>.Create<UIInputDropdown<TValue>>( parent, contextMenuCanvas, layoutInfo, background, backgroundActive, contextMenuBackground, elementBackground, valueToString );
            return parent;
        }

        public static UIInputDropdown<TValue> AddInputDropdown<TValue>( this IUIElementContainer parent, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Sprite backgroundActive, Sprite contextMenuBackground, Sprite elementBackground, Func<TValue, string> valueToString )
        {
            return UIInputDropdown<TValue>.Create<UIInputDropdown<TValue>>( parent, contextMenuCanvas, layoutInfo, background, backgroundActive, contextMenuBackground, elementBackground, valueToString );
        }

        public static UIInputDropdown<string> AddStringInputDropdown( this IUIElementContainer parent, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Sprite backgroundActive, Sprite contextMenuBackground, Sprite elementBackground )
        {
            return UIInputDropdown<string>.Create<UIInputDropdown<string>>( parent, contextMenuCanvas, layoutInfo, background, backgroundActive, contextMenuBackground, elementBackground, s => s );
        }
    }

    public partial class UIInputDropdown<TValue>
    {
        public UIInputDropdown<TValue> WithMargins( float left, float right, float top, float bottom )
        {
            this.textComponent.rectTransform.SetLayoutInfo( new UILayoutInfo( UIFill.Fill( left, right, top, bottom ) ) );
            this.placeholderComponent.rectTransform.SetLayoutInfo( new UILayoutInfo( UIFill.Fill( left, right, top, bottom ) ) );
            return this;
        }

        public UIInputDropdown<TValue> WithFont( TMPro.TMP_FontAsset font, float fontSize, Color fontColor )
        {
            var textComponent = this.textComponent;
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = fontColor;

            var placeholderComponent = this.placeholderComponent;
            placeholderComponent.font = font;
            placeholderComponent.fontSize = fontSize;
            placeholderComponent.color = new Color( fontColor.r, fontColor.g, fontColor.b, fontColor.a * 0.5f );

            this.contextMenuFont = font;
            this.contextMenuFontSize = fontSize;
            this.contextMenuFontColor = fontColor;

            return this;
        }

        public UIInputDropdown<TValue> WithPlaceholder( string placeholderText )
        {
            this.placeholderComponent.text = placeholderText;
            return this;
        }

        public UIInputDropdown<TValue> WithOptions( params TValue[] options )
        {
            this.options = options;
            this.selectedValue = null;
            return this;
        }

        public UIInputDropdown<TValue> WithOptions( IEnumerable<TValue> options )
        {
            this.options = options.ToArray();
            this.selectedValue = null;
            return this;
        }

        public UIInputDropdown<TValue> WithOptions( TValue[] options, int selected )
        {
            if( selected < 0 || selected >= options.Length )
            {
                throw new ArgumentOutOfRangeException( nameof( selected ), $"Selected value index must fall within the option list" );
            }

            this.options = options;
            this.selectedValue = selected;
            this.SyncVisual();
            return this;
        }
    }
}