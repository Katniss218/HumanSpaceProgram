using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    /// <summary>
    /// A layout driver that lays the elements out vertically and sequentially.
    /// </summary>
    public sealed class VerticalLayoutDriver : LayoutDriver
    {
        public enum Direction : byte
        {
            TopToBottom,
            BottomToTop
        }

        /// <summary>
        /// The direction in which the vertical layout will lay the elements.
        /// </summary>
        /// <remarks>
        /// The first child always goes at the beginning, and the last goes at the end of the sequence.
        /// </remarks>
        public Direction Dir { get; set; } = Direction.TopToBottom;

        /// <summary>
        /// The vertical spacing between the child elements, in [px].
        /// </summary>
        public float Spacing { get; set; }

        /// <summary>
        /// If true, the height of the container element will be set to fit the combined height of its contents.
        /// </summary>
        public bool FitToSize { get; set; }

        public override void DoLayout( IUIElementContainer c )
        {
            // lays out the children in a vertical list.
            // doesn't care about the horizontal dimensions at all, just aligns everything to line up.

            foreach( var child in c.Children )
            {
                if( child.rectTransform.anchorMin.y != child.rectTransform.anchorMax.y )
                {
                    throw new InvalidOperationException( $"Can't layout vertically, the child element {c.gameObject.name} fills height." );
                }
            }

            float ySum = 0;
            foreach( var child in c.Children )
            {
                UILayoutInfo layoutInfo = child.rectTransform.GetLayoutInfo();

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

            if( FitToSize )
            {
                if( c.contents.anchorMin.y != c.contents.anchorMax.y )
                {
                    throw new InvalidOperationException( $"Can't fit to size vertically, the container element {c.gameObject.name} fills height." );
                }

                if( ySum != 0 )
                    ySum -= Spacing; // remove the last spacing if there are any elements.

                c.contents.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, ySum );
            }
        }
    }
}