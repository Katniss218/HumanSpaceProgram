using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIScrollBarEx
    {
        public static UIScrollBar AddScrollbar( this UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, bool isVertical )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( scrollView, isVertical ? "uilib-scrollbar-vertical" : "uilib-scrollbar-horizontal", layout );

            if( background != null )
            {
                Image bg = rootGameObject.AddComponent<Image>();
                bg.sprite = background;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = true;
            }

            (GameObject slidingAreaGameObject, RectTransform slidingAreaTransform) = UIHelper.CreateUI( rootTransform, "uilib-scrollbar-slidingarea", UILayoutInfo.Fill() );
            (GameObject handleGameObject, RectTransform handleTransform) = UIHelper.CreateUI( slidingAreaTransform, "uilib-scrollbar-handle", UILayoutInfo.Fill() );

            Image image = handleGameObject.AddComponent<Image>();
            image.sprite = foreground;
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;

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

            UIScrollBar scrollBar = new UIScrollBar( rootTransform, scrollbarComponent );

            scrollView.scrollbarHorizontal = scrollBar;
            return scrollBar;
        }
    }
}