using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    public sealed class VerticalFitToSizeLayoutDriver : LayoutDriver
    {
        public UIElement TargetElement { get; set; }

        public float MarginTop { get; set; }
        public float MarginBottom { get; set; }

        public override void DoLayout( IUILayoutDriven c )
        {
            if( c is IUIElementContainer container )
            {
                if( container.contents.anchorMin.y != container.contents.anchorMax.y )
                {
                    throw new InvalidOperationException( $"Can't fit to size, the container element {c.gameObject.name} fills height." );
                }

                Vector2 size = TargetElement.rectTransform.GetActualSize();

                size.y += MarginTop + MarginBottom;

                container.contents.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, size.y );
            }
        }
    }
}