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
    /// A layout driver that lays the elements out horizontally and sequentially.
    /// </summary>
    public sealed class HorizontalLayoutDriver : LayoutDriver
    {
        public enum Direction : byte
        {
            LeftToRight,
            RightToLeft
        }

        /// <summary>
        /// The direction in which the vertical layout will lay the elements.
        /// </summary>
        /// <remarks>
        /// The first child always goes at the beginning, and the last goes at the end of the sequence.
        /// </remarks>
        public Direction Dir { get; set; } = Direction.LeftToRight;

        /// <summary>
        /// The horizontal spacing between the child elements, in [px].
        /// </summary>
        public float Spacing { get; set; }

        /// <summary>
        /// If true, the height of the container element will be set to fit the combined height of its contents.
        /// </summary>
        public bool FitToSize { get; set; }

        public override void DoLayout( IUILayoutDriven c )
        {
            // lays out the children in a vertical list.
            // doesn't care about the horizontal dimensions at all, just aligns everything to line up.
            if( c is IUIElementContainer container )
            {
                foreach( var child in container.Children )
                {
                    if( child.rectTransform.anchorMin.x != child.rectTransform.anchorMax.x )
                    {
                        throw new InvalidOperationException( $"Can't layout horizontally, the child element {c.gameObject.name} fills width." );
                    }
                }

                float xSum = 0;
                foreach( var child in container.Children )
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

                    if( Dir == Direction.LeftToRight )
                    {
                        // minus, plus, plus (reverse of 'to bottom').
                        vertPos -= (layoutInfo.anchorMax.x) * parentSize.x;
                        vertPos += (layoutInfo.pivot.x) * height;
                        vertPos += xSum;
                    }
                    else if( Dir == Direction.RightToLeft )
                    {
                        // plus, minus, minus, and inverse of anchor/pivot.
                        vertPos += (1 - layoutInfo.anchorMax.x) * parentSize.x;
                        vertPos -= (1 - layoutInfo.pivot.x) * height;
                        vertPos -= xSum;
                    }*/
                    #endregion

                    if( Dir == Direction.LeftToRight )
                    {
                        layoutInfo.anchorMin.x = 0f;
                        layoutInfo.anchorMax.x = 0f;
                        layoutInfo.pivot.x = 0f;
                        layoutInfo.anchoredPosition.x = xSum;
                    }
                    else if( Dir == Direction.RightToLeft )
                    {
                        layoutInfo.anchorMin.x = 1f;
                        layoutInfo.anchorMax.x = 1f;
                        layoutInfo.pivot.x = 1f;
                        layoutInfo.anchoredPosition.x = -xSum;
                    }

                    child.rectTransform.SetLayoutInfo( layoutInfo );

                    xSum += layoutInfo.sizeDelta.x + Spacing; // Y+ towards the top.
                }

                if( FitToSize )
                {
                    if( container.contents.anchorMin.x != container.contents.anchorMax.x )
                    {
                        throw new InvalidOperationException( $"Can't fit to size horizontally, the container element {c.gameObject.name} fills width." );
                    }

                    if( xSum != 0 )
                        xSum -= Spacing; // remove the last spacing if there are any elements.

                    container.contents.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, xSum );
                }
            }
        }
    }
}