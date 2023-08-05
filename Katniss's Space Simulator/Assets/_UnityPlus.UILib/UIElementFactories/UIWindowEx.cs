using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIWindowEx
    {
        public static UIWindow AddWindow( this Canvas parent, UILayoutInfo layoutInfo, Sprite background )
        {
            (GameObject rootGameObject, RectTransform rootTransform) = UIHelper.CreateUI( (RectTransform)parent.transform, "uilib-window", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            return new UIWindow( rootTransform, backgroundComponent );
        }

        public static UIWindow Focusable( this UIWindow window )
        {
            RectTransformFocuser windowDrag = window.gameObject.AddComponent<RectTransformFocuser>();
            windowDrag.UITransform = window.transform;

            return window;
        }

        public static UIWindow Draggable( this UIWindow window )
        {
            RectTransformDragger windowDrag = window.gameObject.AddComponent<RectTransformDragger>();
            windowDrag.UITransform = window.transform;

            return window;
        }

        public static UIWindow WithCloseButton( this UIWindow window, UILayoutInfo layoutInfo, Sprite buttonSprite, out UIButton closeButton )
        {
            closeButton = window.AddButton( layoutInfo, buttonSprite, null );

            RectTransformCloser closer = closeButton.gameObject.AddComponent<RectTransformCloser>();
            closer.ExitButton = closeButton.buttonComponent;
            closer.UITransform = window.transform;

            return window;
        }
    }
}