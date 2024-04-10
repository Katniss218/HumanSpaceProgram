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

        protected internal static T Create<T>( UIScrollView scrollView, UILayoutInfo layout, Sprite background, Sprite foreground, bool isVertical ) where T : UIScrollBar
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUIGameObject( scrollView.rectTransform, isVertical ? $"uilib-{nameof( T )}-vertical" : $"uilib-{nameof( T )}-horizontal", layout );

            if( background != null )
            {
                Image bg = rootGameObject.AddComponent<Image>();
                bg.sprite = background;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = true;
            }

            (GameObject slidingAreaGameObject, RectTransform slidingAreaTransform) = UIElement.CreateUIGameObject( rootTransform, $"uilib-{nameof( T )}-slidingarea", new UILayoutInfo( UIFill.Fill() ) );
            (GameObject handleGameObject, RectTransform handleTransform) = UIElement.CreateUIGameObject( slidingAreaTransform, $"uilib-{nameof( T )}-handle", new UILayoutInfo( UIFill.Fill() ) );

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

            T uiScrollBar = rootGameObject.AddComponent<T>();
            uiScrollBar.parent = scrollView;
            uiScrollBar.scrollbarComponent = scrollbarComponent;

            scrollView.scrollbarHorizontal = uiScrollBar;
            return uiScrollBar;
        }
    }
}