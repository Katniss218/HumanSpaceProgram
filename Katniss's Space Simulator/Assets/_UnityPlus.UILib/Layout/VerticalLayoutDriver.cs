using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    public sealed class VerticalLayoutDriver : LayoutDriver
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

            float ySum = 0;
            foreach( var child in c.Children )
            {
                UILayoutInfo layoutInfo = child.rectTransform.GetLayoutInfo();

                if( layoutInfo.FillsHeight )
                {
                    throw new InvalidOperationException( $"Can't align vertically, a UI element {child.gameObject.name} fills height." );
                }

                #region [Commented out], This doesn't work, but might be useful...
                // - doesn't work because the canvas scaler fucks with the unit sizes or something like that.

                // The desired value for anchoredPosition depends on the child element's vertical anchor and pivot values, its size, and the size of its container.
                // Increasing the pivot moves the UI element down.
                // Increasing the anchor moves the UI element up.
                /*
                float vertPos = 0;
                float height = layoutInfo.sizeDelta.y;

                if( Dir == Direction.TopToBottom )
                {
                    // plus, minus, minus, and inverse of anchor/pivot.
                    vertPos += (1 - layoutInfo.anchorMax.y) * parentSize.y;
                    vertPos -= (1 - layoutInfo.pivot.y) * height;
                    vertPos -= ySum;
                }
                else if( Dir == Direction.BottomToTop )
                {
                    // minus, plus, plus (reverse of 'to bottom').
                    vertPos -= (layoutInfo.anchorMax.y) * parentSize.y;
                    vertPos += (layoutInfo.pivot.y) * height;
                    vertPos += ySum;
                }*/
                #endregion

                if( Dir == Direction.TopToBottom )
                {
                    layoutInfo.anchorMin.y = 1f;
                    layoutInfo.anchorMax.y = 1f;
                    layoutInfo.pivot.y = 1f;
                    layoutInfo.anchoredPosition.y = -ySum;
                }
                else if( Dir == Direction.BottomToTop )
                {
                    layoutInfo.anchorMin.y = 0f;
                    layoutInfo.anchorMax.y = 0f;
                    layoutInfo.pivot.y = 0f;
                    layoutInfo.anchoredPosition.y = ySum;
                }
               
                child.rectTransform.SetLayoutInfo( layoutInfo );

                ySum += layoutInfo.sizeDelta.y + Spacing; // Y+ towards the top.
            }
        }
    }
}