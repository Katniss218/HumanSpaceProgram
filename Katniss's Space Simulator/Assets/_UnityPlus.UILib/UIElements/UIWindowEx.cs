using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIWindowEx
    {
        public static UIWindow AddWindow( this UICanvas parent, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUI( parent.rectTransform, "uilib-window", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            return new UIWindow( rootTransform, parent, backgroundComponent );
        }

        public static UIWindow Focusable( this UIWindow window )
        {
            RectTransformFocuser windowDrag = window.gameObject.AddComponent<RectTransformFocuser>();
            windowDrag.UITransform = window.rectTransform;

            return window;
        }

        public static UIWindow Draggable( this UIWindow window )
        {
            RectTransformDragger windowDrag = window.gameObject.AddComponent<RectTransformDragger>();
            windowDrag.UITransform = window.rectTransform;

            return window;
        }

        public static UIWindow WithCloseButton( this UIWindow window, UILayoutInfo layoutInfo, Sprite buttonSprite, out UIButton closeButton )
        {
            closeButton = window.AddButton( layoutInfo, buttonSprite, null );

            RectTransformCloser closer = closeButton.gameObject.AddComponent<RectTransformCloser>();
            closer.ExitButton = closeButton.buttonComponent;
            closer.UITransform = window.rectTransform;

            return window;
        }
    }
}