using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIScrollViewEx
    {
        public static UIScrollView AddHorizontalScrollView( this IUIElementContainer parent, UILayoutInfo layout, float contentWidth )
        {
            return AddScrollView( parent, layout, UILayoutInfo.FillVertical( 0, 0, 0.0f, 0, contentWidth ), true, false );
        }

        public static UIScrollView AddVerticalScrollView( this IUIElementContainer parent, UILayoutInfo layout, float contentHeight )
        {
            return AddScrollView( parent, layout, UILayoutInfo.FillHorizontal( 0, 0, 1.0f, 0, contentHeight ), false, true );
        }

        public static UIScrollView AddScrollView( this IUIElementContainer parent, UILayoutInfo layout, UILayoutInfo contentLayout, bool horizontal, bool vertical )
        {
            return UIScrollView.Create( parent, layout, contentLayout, horizontal, vertical );
        }

        public static UIScrollView WithSensitivity( this UIScrollView scrollView, float sensitivity, float deceleration )
        {
            var scrollRectComponent = scrollView.scrollRectComponent;
            scrollRectComponent.movementType = ScrollRect.MovementType.Clamped;
            scrollRectComponent.inertia = true;
            scrollRectComponent.scrollSensitivity = sensitivity;
            scrollRectComponent.decelerationRate = deceleration;

            return scrollView;
        }

        public static UIScrollView WithHorizontalScrollbar( this UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, out UIScrollBar scrollBar )
        {
#warning TODO - the contents should take the margin around the scrollbars. Also, the scrollbars should be more restricted in size. only the "free" axis is allowed to be set, and bottom/top margin depends on the presence and the size of the other scrollbar.
            scrollBar = scrollView.AddScrollbar( layout, background, foreground, false );
            return scrollView;
        }

        public static UIScrollView WithVerticalScrollbar( this UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, out UIScrollBar scrollBar )
        {
            scrollBar = scrollView.AddScrollbar( layout, background, foreground, true );
            return scrollView;
        }
    }
}