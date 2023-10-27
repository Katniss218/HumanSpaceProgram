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

        static UIScrollView AddScrollView( this IUIElementContainer parent, UILayoutInfo layout, UILayoutInfo contentLayout, bool horizontal, bool vertical )
        {
            (GameObject root, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-scrollview", layout );

            (GameObject viewport, RectTransform viewportTransform) = UIElement.CreateUI( rootTransform, "uilib-scrollviewviewport", UILayoutInfo.Fill() );

            Image maskImage = viewport.AddComponent<Image>();
            maskImage.maskable = true;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject content, RectTransform contentTransform) = UIElement.CreateUI( viewportTransform, "uilib-scrollviewcontent", contentLayout );

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.content = (RectTransform)content.transform;
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.horizontalScrollbarSpacing = 0.0f;
            scrollRect.verticalScrollbarSpacing = 0.0f;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.decelerationRate = 0.5f;

            return new UIScrollView( rootTransform, parent, scrollRect, null, null, contentTransform );
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