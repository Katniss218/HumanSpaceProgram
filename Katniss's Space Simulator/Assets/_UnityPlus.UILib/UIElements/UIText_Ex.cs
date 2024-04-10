using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public static class UIText_Ex
    {
        public static T WithText<T>( this T parent, UILayoutInfo layoutInfo, string text, out UIText uiText ) where T : IUIElementContainer
        {
            uiText = UIText.Create<UIText>( parent, layoutInfo, text );
            return parent;
        }

        public static UIText AddText( this IUIElementContainer parent, UILayoutInfo layoutInfo, string text )
        {
            return UIText.Create<UIText>( parent, layoutInfo, text );
        }
    }

    public partial class UIText
    {
        public UIText WithFont( TMPro.TMP_FontAsset font, float fontSize, Color color )
        {
            var textComponent = this.textComponent;
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = color;

            return this;
        }

        public UIText WithAlignment( TMPro.HorizontalAlignmentOptions horizontalAlignment )
        {
            this.textComponent.horizontalAlignment = horizontalAlignment;
            return this;
        }

        public UIText WithAlignment( TMPro.VerticalAlignmentOptions verticalAlignment )
        {
            this.textComponent.verticalAlignment = verticalAlignment;
            return this;
        }

        public UIText WithAlignment( TMPro.HorizontalAlignmentOptions horizontalAlignment, TMPro.VerticalAlignmentOptions verticalAlignment )
        {
            var textComponent = this.textComponent;
            textComponent.horizontalAlignment = horizontalAlignment;
            textComponent.verticalAlignment = verticalAlignment;

            return this;
        }
    }
}