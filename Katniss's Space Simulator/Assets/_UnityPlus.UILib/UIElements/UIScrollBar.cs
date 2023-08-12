using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIScrollBar : UIElement
    {
        internal readonly UnityEngine.UI.Scrollbar scrollbarComponent;

        public UIScrollBar( RectTransform transform, UnityEngine.UI.Scrollbar scrollbarComponent ) : base( transform )
        {
            this.scrollbarComponent = scrollbarComponent;
        }
    }
}