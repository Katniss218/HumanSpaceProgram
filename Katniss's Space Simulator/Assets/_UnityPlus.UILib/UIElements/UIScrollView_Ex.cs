using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIScrollView_Ex
    {
        public static UIScrollView AddHorizontalScrollView( this IUIElementContainer parent, UILayoutInfo layout, float contentWidth )
        {
            return AddScrollView( parent, layout, new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 0, contentWidth ), true, false );
        }

        public static UIScrollView AddVerticalScrollView( this IUIElementContainer parent, UILayoutInfo layout, float contentHeight )
        {
            return AddScrollView( parent, layout, new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, contentHeight ), false, true );
        }

        public static UIScrollView AddScrollView( this IUIElementContainer parent, UILayoutInfo layout, UILayoutInfo contentLayout, bool horizontal, bool vertical )
        {
            return UIScrollView.Create<UIScrollView>( parent, layout, contentLayout, horizontal, vertical );
        }
    }

    public partial class UIScrollView
    {
        public UIScrollView WithSensitivity( float sensitivity )
        {
            this.scrollRectComponent.scrollSensitivity = sensitivity;
            return this;
        }

        public UIScrollView WithHorizontalScrollbar( UILayoutInfo layout, Sprite background, Sprite foreground, out UIScrollBar scrollBar )
        {
#warning TODO - the contents should take the margin around the scrollbars. Also, the scrollbars should be more restricted in size. only the "free" axis is allowed to be set, and bottom/top margin depends on the presence and the size of the other scrollbar.
            scrollBar = this.AddScrollbar( layout, background, foreground, false );
            return this;
        }

        public UIScrollView WithVerticalScrollbar( UILayoutInfo layout, Sprite background, Sprite foreground, out UIScrollBar scrollBar )
        {
            scrollBar = this.AddScrollbar( layout, background, foreground, true );
            return this;
        }
    }
}