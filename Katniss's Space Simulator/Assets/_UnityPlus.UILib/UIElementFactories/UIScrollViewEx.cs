using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIScrollViewEx
    {
        public static UIScrollView AddHorizontalScrollView( this UIElement parent, UILayoutInfo layout, Vector2 contentSize )
        {
            return AddScrollView( parent, layout, contentSize, true, false );
        }

        public static UIScrollView AddVerticalScrollView( this UIElement parent, UILayoutInfo layout, Vector2 contentSize )
        {
            return AddScrollView( parent, layout, contentSize, false, true );
        }

        public static UIScrollView AddScrollView( this UIElement parent, UILayoutInfo layout, Vector2 contentSize, bool horizontal, bool vertical )
        {
            (GameObject root, RectTransform rootTransform) = UIHelper.CreateUI( parent, "uilib-scrollview", layout );

            (GameObject viewport, RectTransform viewportTransform) = UIHelper.CreateUI( rootTransform, "uilib-scrollviewviewport", UILayoutInfo.Fill() );

            Image maskImage = viewport.AddComponent<Image>();
            maskImage.maskable = true;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject content, RectTransform contentTransform) = UIHelper.CreateUI( viewportTransform, "uilib-scrollviewcontent", new UILayoutInfo( new Vector2( 0.0f, 1.0f ), new Vector2( 1, 1 ), Vector2.zero, contentSize ) );

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

            return new UIScrollView( rootTransform, scrollRect, null, null, new UIElement( contentTransform ) );
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