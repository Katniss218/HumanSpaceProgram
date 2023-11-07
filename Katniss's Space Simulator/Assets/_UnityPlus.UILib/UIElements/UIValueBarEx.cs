using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIValueBarEx
    {
        public static UIValueBar AddHorizontalValueBar( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIValueBar.Create( parent, layoutInfo, background );
        }

        public static UIValueBar WithPadding( this UIValueBar valueBar, float paddingleft, float paddingRight, float spacing )
        {
            var valueBarComponent = valueBar.valueBarComponent;
            valueBarComponent.PaddingLeft = paddingleft;
            valueBarComponent.PaddingRight = paddingRight;
            valueBarComponent.Spacing = spacing;

            return valueBar;
        }
    }
}