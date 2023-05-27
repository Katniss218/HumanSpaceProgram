using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public static class TextFactory
    {
        public static (RectTransform t, TMPro.TextMeshProUGUI text)
            CreateText( RectTransform parent, string name, string text, TMPro.HorizontalAlignmentOptions horizontalAlignment, TMPro.VerticalAlignmentOptions verticalAlignment, UILayoutInfo layoutInfo, UIStyle style )
        {
            (GameObject rootGO, RectTransform rootT) = UIHelper.CreateUI( parent, name, layoutInfo );

            TMPro.TextMeshProUGUI textElem = rootGO.AddComponent<TMPro.TextMeshProUGUI>();
            textElem.raycastTarget = false;
            textElem.richText = false;
            textElem.color = style.TextColor;
            textElem.font = style.TextFont;
            textElem.fontSize = style.TextFontSize;
            textElem.horizontalAlignment = horizontalAlignment;
            textElem.verticalAlignment = verticalAlignment;

            textElem.text = text;

            return (rootT, textElem);
        }
    }
}