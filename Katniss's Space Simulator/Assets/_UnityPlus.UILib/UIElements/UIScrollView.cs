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

        RectTransform _contentTransform;
        public virtual RectTransform contents { get => _contentTransform; }

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, UILayoutInfo contentLayout, bool horizontal, bool vertical ) where T : UIScrollView
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiScrollView) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{nameof( T )}", layout );

            (GameObject viewport, RectTransform viewportTransform) = UIElement.CreateUIGameObject( rootTransform, $"uilib-{nameof( T )}-viewport", new UILayoutInfo( UIFill.Fill() ) );

            Image maskImage = viewport.AddComponent<Image>();
            maskImage.maskable = true;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject content, RectTransform contentTransform) = UIElement.CreateUIGameObject( viewportTransform, $"uilib-{nameof( T )}-content", contentLayout );

            ScrollRect scrollRect = rootGameObject.AddComponent<ScrollRect>();
            scrollRect.content = (RectTransform)content.transform;
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
            uiScrollView._contentTransform = contentTransform;
            return uiScrollView;
        }
    }
}