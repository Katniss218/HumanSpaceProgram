using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public partial class UIScrollView : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        protected internal ScrollRect scrollRectComponent;

        public UIScrollBar scrollbarHorizontal;
        public UIScrollBar scrollbarVertical;

        RectTransform _contentsTransform;
        public virtual RectTransform contents { get => _contentsTransform; }

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        /*public virtual void SetContentsSize( float left, float right, float top, float bottom )
        {
            Vector2 containerSize = this.rectTransform.rect.size;

            Vector2 contentsCenter = new Vector2( (left + right) / 2, (top + bottom) / 2 ); // center relative to pivot

            // TODO - The children are offset from the container, but the container size is correct. I don't know why.
            // we have to update the xy anchor of each child because the anchor doesn't match up with where it was.
            // we essentially want to keep the anchor where the center was, but extend the bounds asymmetrically

            _contentsTransform.sizeDelta = new Vector2( (right - left), (top - bottom) );
            scrollRectComponent.normalizedPosition = contentsCenter / containerSize;
        }*/

        private static void FixPositionRecursive( RectTransform rectTransform )
        {

        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, UILayoutInfo contentsLayout, bool horizontal, bool vertical ) where T : UIScrollView
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiScrollView) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layout );

            (GameObject viewport, RectTransform viewportTransform) = UIElement.CreateUIGameObject( rootTransform, $"uilib-{typeof( T ).Name}-viewport", new UILayoutInfo( UIFill.Fill() ) );

            Image maskImage = viewport.AddComponent<Image>();
            maskImage.maskable = true;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject contents, RectTransform contentsTransform) = UIElement.CreateUIGameObject( viewportTransform, $"uilib-{typeof( T ).Name}-content", contentsLayout );

            ScrollRect scrollRect = rootGameObject.AddComponent<ScrollRect>();
            scrollRect.content = (RectTransform)contents.transform;
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.horizontalScrollbarSpacing = 0.0f;
            scrollRect.verticalScrollbarSpacing = 0.0f;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;
            scrollRect.scrollSensitivity = 30f;

            uiScrollView.scrollRectComponent = scrollRect;
            uiScrollView.scrollbarHorizontal = null;
            uiScrollView.scrollbarVertical = null;
            uiScrollView._contentsTransform = contentsTransform;
            return uiScrollView;
        }
    }
}