using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UITextEx
    {
        public static UIText AddText( this UIElement parent, UILayoutInfo layoutInfo, string text )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-text", layoutInfo );

            TMPro.TextMeshProUGUI textComponent = rootGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.raycastTarget = false;
            textComponent.richText = false;
            textComponent.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Left;
            textComponent.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;

            textComponent.text = text;

            return new UIText( rootTransform, textComponent );
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