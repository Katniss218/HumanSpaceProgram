using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public sealed class UIScrollView : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal ScrollRect scrollRectComponent;

        public UIScrollBar scrollbarHorizontal;
        public UIScrollBar scrollbarVertical;
        RectTransform _contents;
        public RectTransform contents { get => _contents; }

        public List<IUIElementChild> Children { get; private set; }

        public IUIElementContainer Parent { get; private set; }

        public LayoutDriver LayoutDriver { get; set; }

        void OnDestroy()
        {
            this.Parent.Children.Remove( this );
        }

        public static UIScrollView Create( IUIElementContainer parent, UILayoutInfo layout, UILayoutInfo contentLayout, bool horizontal, bool vertical )
        {
            (GameObject root, RectTransform rootTransform) = UIElement.CreateUI( parent.contents, "uilib-scrollview", layout );

            (GameObject viewport, RectTransform viewportTransform) = UIElement.CreateUI( rootTransform, "uilib-scrollviewviewport", UILayoutInfo.Fill() );

            Image maskImage = viewport.AddComponent<Image>();
            maskImage.maskable = true;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject content, RectTransform contentTransform) = UIElement.CreateUI( viewportTransform, "uilib-scrollviewcontent", contentLayout );

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.content = (RectTransform)content.transform;
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.horizontalScrollbarSpacing = 0.0f;
            scrollRect.verticalScrollbarSpacing = 0.0f;

            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.decelerationRate = 0.5f;

            UIScrollView uiScrollView = root.AddComponent<UIScrollView>();

            uiScrollView. Children = new List<IUIElementChild>();
            uiScrollView.Parent = parent;
            uiScrollView.Parent.Children.Add( uiScrollView );
            uiScrollView.scrollRectComponent = scrollRect;
            uiScrollView.scrollbarHorizontal = null;
            uiScrollView.scrollbarVertical = null;
            uiScrollView._contents = contentTransform;
            return uiScrollView;
        }
    }
}