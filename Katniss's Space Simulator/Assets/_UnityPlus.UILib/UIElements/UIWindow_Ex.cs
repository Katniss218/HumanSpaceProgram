using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    public static class UIWindow_Ex
    {
        public static UIWindow AddWindow( this UICanvas parent, UILayoutInfo layoutInfo, Sprite background )
        {
            return UIWindow.Create<UIWindow>( parent, layoutInfo, background );
        }
    }

    public partial class UIWindow
    {
        public UIWindow Focusable()
        {
            RectTransformFocuser windowDrag = this.gameObject.AddComponent<RectTransformFocuser>();
            windowDrag.UITransform = this.rectTransform;

            return this;
        }

        public UIWindow Draggable()
        {
            RectTransformDragger windowDrag = this.gameObject.AddComponent<RectTransformDragger>();
            windowDrag.UITransform = this.rectTransform;

            return this;
        }

        /// <summary>
        /// A shorthand for AddButton that closes the window.
        /// </summary>
        public UIWindow WithCloseButton( UILayoutInfo layoutInfo, Sprite buttonSprite, out UIButton closeButton )
        {
            closeButton = this.AddButton( layoutInfo, buttonSprite, () =>
            {
                this.Destroy();
            } );

            return this;
        }
    }
}