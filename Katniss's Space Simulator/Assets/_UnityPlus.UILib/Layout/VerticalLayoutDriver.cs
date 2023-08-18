using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    public class VerticalLayoutDriver : LayoutDriver
    {
        public enum Direction : byte
        {
            TopToBottom,
            BottomToTop
        }

        public Direction Dir { get; set; } = Direction.TopToBottom;

        public float Spacing { get; set; }

        // TODO - do we do fit to size here or somewhere else? here would reduce code duplication

        public override void DoLayout( IUIElementContainer c )
        {
            // lays out the children in a vertical list.
            // doesn't care about the horizontal dimensions at all, just aligns everything to line up.

            UILayoutInfo parentLayoutInfo = c.rectTransform.GetLayoutInfo();

            float ySum = 0;
            foreach( var child in c.Children )
            {
                UILayoutInfo layoutInfo = child.rectTransform.GetLayoutInfo();

                if( layoutInfo.FillsHeight )
                {
                    throw new InvalidOperationException( $"Can't align vertically, a UI element {child.gameObject.name} fills height." );
                }

                // The desired value for anchoredPosition depends on the child element's vertical anchor and pivot values, its size, and the size of its container.
                // Increasing the pivot moves the UI element down.
                // Increasing the anchor moves the UI element up.

                float vertPos = 0;
                float height = child.rectTransform.sizeDelta.y;

                if( Dir == Direction.TopToBottom )
                {
                    // plus, minus, minus, and inverse of anchor/pivot.
                    vertPos += (1 - layoutInfo.anchorMax.y) * parentLayoutInfo.sizeDelta.y; // Relative to anchor -> absolute.
                    vertPos -= (1 - layoutInfo.pivot.y) * layoutInfo.sizeDelta.y;           // Relative to pivot -> absolute.
                    vertPos -= ySum;                                                        // Absolute -> absolute
                }
                else if( Dir == Direction.BottomToTop )
                {
                    // minus, plus, plus (reverse of 'to bottom').
                    vertPos -= (layoutInfo.anchorMax.y) * parentLayoutInfo.sizeDelta.y;
                    vertPos += (layoutInfo.pivot.y) * layoutInfo.sizeDelta.y;
                    vertPos += ySum;
                }

                layoutInfo.anchoredPosition.x = vertPos;

                ySum += height + Spacing; // Y+ towards the top.
            }
        }
    }
}
