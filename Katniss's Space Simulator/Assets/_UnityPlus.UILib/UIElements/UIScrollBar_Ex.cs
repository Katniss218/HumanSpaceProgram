using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIScrollBar_Ex
    {
        public static UIScrollBar AddScrollbar( this UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, bool isVertical )
        {
            return UIScrollBar.Create<UIScrollBar>( scrollView, layout, background, foreground, isVertical );
        }
    }
}