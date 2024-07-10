using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public class UIScrollBar : UIElement
    {
        protected Scrollbar scrollbarComponent;

        protected UIScrollView parent;

        protected internal static T Create<T>( UIScrollView scrollView, UIAnchor anchor, UISize size, Sprite background, Sprite foreground, bool isVertical ) where T : UIScrollBar
        {
            UILayoutInfo layout;
            if( isVertical )
            {
                float rightAndBottom = anchor.x == UIAnchor.Left.x ? 0 : size.y;
                float leftAndTop = anchor.x == UIAnchor.Right.x ? 0 : size.y;

                scrollView.viewport.SetHorizontalMargins( leftAndTop, rightAndBottom );
                if( scrollView.scrollbarHorizontal != null )
                {
                    scrollView.scrollbarHorizontal.rectTransform.SetHorizontalMargins( leftAndTop, rightAndBottom );
                    layout = new UILayoutInfo( (UIAnchorHorizontal)anchor, UIFill.Vertical( leftAndTop, rightAndBottom ), 0, size.y );
                }
                else
                {
                    layout = new UILayoutInfo( (UIAnchorHorizontal)anchor, UIFill.Vertical(), 0, size.y );
                }
            }
            else
            {
                float rightAndBottom = anchor.y == UIAnchor.Top.y ? 0 : size.x;
                float leftAndTop = anchor.y == UIAnchor.Bottom.y ? 0 : size.x;

                scrollView.viewport.SetVerticalMargins( leftAndTop, rightAndBottom );
                if( scrollView.scrollbarVertical != null )
                {
                    scrollView.scrollbarVertical.rectTransform.SetVerticalMargins( leftAndTop, rightAndBottom );
                    layout = new UILayoutInfo( UIFill.Horizontal( leftAndTop, rightAndBottom ), (UIAnchorVertical)anchor, 0, size.x );
                }
                else
                {
                    layout = new UILayoutInfo( UIFill.Horizontal(), (UIAnchorVertical)anchor, 0, size.x );
                }
            }

            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUIGameObject( scrollView.rectTransform, isVertical ? $"uilib-{typeof( T ).Name}-vertical" : $"uilib-{typeof( T ).Name}-horizontal", layout );

            if( background != null )
            {
                Image bg = rootGameObject.AddComponent<Image>();
                bg.sprite = background;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = true;
            }

            (GameObject slidingAreaGameObject, RectTransform slidingAreaTransform) = UIElement.CreateUIGameObject( rootTransform, $"uilib-{typeof( T ).Name}-slidingarea", new UILayoutInfo( UIFill.Fill() ) );
            (GameObject handleGameObject, RectTransform handleTransform) = UIElement.CreateUIGameObject( slidingAreaTransform, $"uilib-{typeof( T ).Name}-handle", new UILayoutInfo( UIFill.Fill() ) );

            Image handleImage = handleGameObject.AddComponent<Image>();
            handleImage.sprite = foreground;
            handleImage.type = Image.Type.Sliced;
            handleImage.raycastTarget = true;

            Scrollbar scrollbarComponent = rootGameObject.AddComponent<Scrollbar>();
            scrollbarComponent.transition = Selectable.Transition.ColorTint;
            scrollbarComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };
            scrollbarComponent.handleRect = handleTransform;
            scrollbarComponent.direction = isVertical ? Scrollbar.Direction.BottomToTop : Scrollbar.Direction.LeftToRight;
            scrollbarComponent.targetGraphic = handleImage;

            T uiScrollBar = rootGameObject.AddComponent<T>();
            uiScrollBar.parent = scrollView;
            uiScrollBar.scrollbarComponent = scrollbarComponent;

            if( isVertical )
            {
                scrollView.scrollbarVertical = uiScrollBar;
                scrollView.scrollRectComponent.verticalScrollbar = scrollbarComponent;
            }
            else
            {
                scrollView.scrollbarHorizontal = uiScrollBar;
                scrollView.scrollRectComponent.horizontalScrollbar = scrollbarComponent;
            }

            return uiScrollBar;
        }
    }
}