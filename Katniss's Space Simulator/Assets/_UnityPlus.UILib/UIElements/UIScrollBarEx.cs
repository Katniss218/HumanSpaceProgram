using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIScrollBarEx
    {
        public static UIScrollBar AddScrollbar( this UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, bool isVertical )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( scrollView.rectTransform, isVertical ? "uilib-scrollbar-vertical" : "uilib-scrollbar-horizontal", layout );

            if( background != null )
            {
                Image bg = rootGameObject.AddComponent<Image>();
                bg.sprite = background;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = true;
            }

            (GameObject slidingAreaGameObject, RectTransform slidingAreaTransform) = UIElement.CreateUI( rootTransform, "uilib-scrollbar-slidingarea", UILayoutInfo.Fill() );
            (GameObject handleGameObject, RectTransform handleTransform) = UIElement.CreateUI( slidingAreaTransform, "uilib-scrollbar-handle", UILayoutInfo.Fill() );

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

            if( isVertical )
                scrollView.scrollRectComponent.verticalScrollbar = scrollbarComponent;
            else
                scrollView.scrollRectComponent.horizontalScrollbar = scrollbarComponent;

            UIScrollBar scrollBar = new UIScrollBar( rootTransform, scrollView, scrollbarComponent );

            scrollView.scrollbarHorizontal = scrollBar;
            return scrollBar;
        }
    }
}