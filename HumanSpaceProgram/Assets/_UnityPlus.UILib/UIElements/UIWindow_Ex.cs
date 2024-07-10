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
            RectTransformFocuser focuser = this.gameObject.AddComponent<RectTransformFocuser>();

            RectTransformDragResize dragger = this.rectTransform.parent != null
                ? this.rectTransform.parent.GetComponent<RectTransformDragResize>()
                : null;

            if( dragger == null )
            {
                focuser.UITransform = this.rectTransform;
            }
            else
            {
                focuser.UITransform = (RectTransform)dragger.transform;
            }

            return this;
        }

        public UIWindow Draggable()
        {
            RectTransformDragMove dragger = this.gameObject.AddComponent<RectTransformDragMove>();

            RectTransformDragResize resizer = this.rectTransform.parent != null 
                ? this.rectTransform.parent.GetComponent<RectTransformDragResize>()
                : null;

            if( resizer == null )
            {
                dragger.UITransform = this.rectTransform;
            }
            else
            {
                dragger.UITransform = (RectTransform)resizer.transform;
            }

            return this;
        }

        public UIWindow Resizeable( float margin = 8f )
        {
            if( this.rectTransform.FillsWidth() || this.rectTransform.FillsHeight() )
            {
                throw new InvalidOperationException( $"Can't make resizable a window that fills width or height." );
            }

            UILayoutInfo layoutInfo = this.rectTransform.GetLayoutInfo();
            layoutInfo.sizeDelta += new Vector2( margin, margin );

            (GameObject rootGameObject, RectTransform rootTransform) = UIElement.CreateUIGameObject( (RectTransform)this.rectTransform.parent, $"uilib-{typeof( UIWindow ).Name}-resizemargin", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = true;
            imageComponent.sprite = null;
            imageComponent.color = new Color( 0, 0, 0, 0 );

            RectTransformDragResize resizer = rootGameObject.AddComponent<RectTransformDragResize>();
            resizer.Padding = 2 * margin; // twice makes it easier to grab the corners, and to grab the window if it doesn't have a separate dragger

            this.transform.SetParent( rootTransform );
            ((RectTransform)this.transform).SetLayoutInfo( new UILayoutInfo( UIFill.Fill( margin, margin, margin, margin ) ) );
            this.OnDestroyListener += () => Destroy( resizer.gameObject );

            RectTransformFocuser focuser = this.gameObject.GetComponent<RectTransformFocuser>();
            if( focuser != null )
            {
                focuser.UITransform = rootTransform;
            }

            RectTransformDragMove dragger = this.gameObject.GetComponent<RectTransformDragMove>();
            if( dragger != null )
            {
                dragger.UITransform = rootTransform;
            }

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