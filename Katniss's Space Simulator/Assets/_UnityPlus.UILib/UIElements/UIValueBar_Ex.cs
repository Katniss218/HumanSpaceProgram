using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIValueBar_Ex
    {
        public static UIValueBar AddHorizontalValueBar( this IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIValueBar.Create<UIValueBar>( parent, layoutInfo, background );
        }
    }

    public partial class UIValueBar
    {
        public UIValueBar WithPadding( float paddingleft, float paddingRight, float spacing )
        {
            var valueBarComponent = this.valueBarComponent;
            valueBarComponent.PaddingLeft = paddingleft;
            valueBarComponent.PaddingRight = paddingRight;
            valueBarComponent.Spacing = spacing;

            return this;
        }
    }
}