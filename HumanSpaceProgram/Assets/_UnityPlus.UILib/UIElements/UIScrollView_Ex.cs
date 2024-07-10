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
            return UIScrollView.Create<UIScrollView>( parent, layout, new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 0, contentWidth ), true, false );
        }

        public static UIScrollView AddVerticalScrollView( this IUIElementContainer parent, UILayoutInfo layout, float contentHeight )
        {
            return UIScrollView.Create<UIScrollView>( parent, layout, new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, contentHeight ), false, true );
        }

        public static UIScrollView AddScrollView( this IUIElementContainer parent, UILayoutInfo layout, UISize contentSize, bool horizontal, bool vertical )
        {
            return UIScrollView.Create<UIScrollView>( parent, layout, new UILayoutInfo( UIAnchor.Center, (0, 0), contentSize ), horizontal, vertical );
        }
    }

    public partial class UIScrollView
    {
        public UIScrollView WithSensitivity( float sensitivity )
        {
            this.scrollRectComponent.scrollSensitivity = sensitivity;
            return this;
        }

        public UIScrollView WithHorizontalScrollbar( UIAnchorVertical anchor, float width, Sprite background, Sprite foreground, out UIScrollBar scrollBar )
        {
            scrollBar = UIScrollBar.Create<UIScrollBar>( this, anchor, (width, 0), background, foreground, false );
            return this;
        }

        public UIScrollView WithVerticalScrollbar( UIAnchorHorizontal anchor, float height, Sprite background, Sprite foreground, out UIScrollBar scrollBar )
        {
            scrollBar = UIScrollBar.Create<UIScrollBar>( this, anchor, (0, height), background, foreground, true );
            return this;
        }
    }
}