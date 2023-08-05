using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIScrollView : UIElement
    {
        public UnityEngine.UI.ScrollRect scrollRectComponent;

        public UIScrollBar scrollbarHorizontal;
        public UIScrollBar scrollbarVertical;
        public UIElement contents;

        public UIScrollView( RectTransform transform, UnityEngine.UI.ScrollRect scrollRectComponent, UIScrollBar scrollbarHorizontal, UIScrollBar scrollbarVertical, UIElement contents ) : base( transform )
        {
            this.scrollRectComponent = scrollRectComponent;
            this.scrollbarHorizontal = scrollbarHorizontal;
            this.scrollbarVertical = scrollbarVertical;
            this.contents = contents;
        }
    }
}