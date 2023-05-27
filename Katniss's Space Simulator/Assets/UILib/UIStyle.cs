using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    [CreateAssetMenu( fileName = "style", menuName = "Generic UI Style", order = 100 )]
    public class UIStyle : ScriptableObject
    {
        public TMPro.TMP_FontAsset TextFont;
        public Color TextColor;
        public float TextFontSize;

        public Color ButtonHover;
        public Color ButtonClick;
        public Color ButtonDisabled;

        public Sprite Button2Axis;
        public Sprite InputField;

        public float BarSpacing;
        public float BarPaddingLeft;
        public float BarPaddingRight;
        public Sprite Bar;
        public Sprite BarBackground;
    }
}