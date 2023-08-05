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

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = true;
            imageComponent.sprite = background;
            imageComponent.type = Image.Type.Filled;

            return new UIWindow( rootTransform, imageComponent );
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

            RectTransformCloser closer = window.gameObject.AddComponent<RectTransformCloser>();
            closer.ExitButton = closeButton.buttonComponent;
            closer.UITransform = window.transform;

            return window;
        }
    }
}