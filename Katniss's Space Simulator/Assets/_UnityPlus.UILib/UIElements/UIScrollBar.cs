using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIScrollBar : UIElement
    {
        internal Scrollbar scrollbarComponent;

        UIScrollView _parent;

        public static UIScrollBar Create( UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, bool isVertical )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUIGameObject( scrollView.rectTransform, isVertical ? "uilib-scrollbar-vertical" : "uilib-scrollbar-horizontal", layout );

            if( background != null )
            {
                Image bg = rootGameObject.AddComponent<Image>();
                bg.sprite = background;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = true;
            }

            (GameObject slidingAreaGameObject, RectTransform slidingAreaTransform) = UIElement.CreateUIGameObject( rootTransform, "uilib-scrollbar-slidingarea", UILayoutInfo.Fill() );
            (GameObject handleGameObject, RectTransform handleTransform) = UIElement.CreateUIGameObject( slidingAreaTransform, "uilib-scrollbar-handle", UILayoutInfo.Fill() );

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

            UIScrollBar uiScrollBar = rootGameObject.AddComponent<UIScrollBar>();
            uiScrollBar._parent = scrollView;
            uiScrollBar.scrollbarComponent = scrollbarComponent;

            scrollView.scrollbarHorizontal = uiScrollBar;
            return uiScrollBar;
        }
    }
}