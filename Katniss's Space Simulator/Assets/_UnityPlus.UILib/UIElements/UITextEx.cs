using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public static class UITextEx
    {
        public static T WithText<T>( this T parent, UILayoutInfo layoutInfo, string text, out UIText uiText ) where T : IUIElementContainer
        {
            uiText = UIText.Create( parent, layoutInfo, text );
            return parent;
        }

        public static UIText AddText( this IUIElementContainer parent, UILayoutInfo layoutInfo, string text )
        {
            return UIText.Create( parent, layoutInfo, text );
        }

        public static UIText WithFont( this UIText text, TMPro.TMP_FontAsset font, float fontSize, Color color )
        {
            var textComponent = text.textComponent;
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = color;

            return text;
        }

        public static UIText WithAlignment( this UIText text, TMPro.HorizontalAlignmentOptions horizontalAlignment )
        {
            text.textComponent.horizontalAlignment = horizontalAlignment;
            return text;
        }

        public static UIText WithAlignment( this UIText text, TMPro.VerticalAlignmentOptions verticalAlignment )
        {
            text.textComponent.verticalAlignment = verticalAlignment;
            return text;
        }

        public static UIText WithAlignment( this UIText text, TMPro.HorizontalAlignmentOptions horizontalAlignment, TMPro.VerticalAlignmentOptions verticalAlignment )
        {
            var textComponent = text.textComponent;
            textComponent.horizontalAlignment = horizontalAlignment;
            textComponent.verticalAlignment = verticalAlignment;

            return text;
        }
    }
}