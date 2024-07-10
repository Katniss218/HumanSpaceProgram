using System;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    /// <summary>
    /// A layout driver that lays the elements out vertically and horizontally.
    /// </summary>
    public sealed class BidirectionalLayoutDriver : LayoutDriver
    {
        public enum DirectionY : byte
        {
            TopToBottom,
            BottomToTop
        }
        public enum DirectionX : byte
        {
            LeftToRight,
            RightToLeft
        }
        public enum Axis2D : byte
        {
            X = 0,
            Y = 1
        }

        public DirectionX DirX { get; set; } = DirectionX.LeftToRight;
        public DirectionY DirY { get; set; } = DirectionY.TopToBottom;

        /// <summary>
        /// The axis that is free to expand indefinitely. <br />
        /// The layout driver will place the elements on the other axis first, and when it's filled up, it will move over along this axis.
        /// </summary>
        public Axis2D FreeAxis { get; set; } = Axis2D.Y;

        /// <summary>
        /// The spacing between the child elements, in [px].
        /// </summary>
        public Vector2 Spacing { get; set; }

        /// <summary>
        /// If true, the height of the container element will be set to fit the combined height of its contents.
        /// </summary>
        public bool FitToSize { get; set; } // fit to size is only applied on the axis different from primary.

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
                        throw new InvalidOperationException( $"Can't layout in a grid, the child element {c.gameObject.name} fills width." );
                    }
                    if( child.rectTransform.anchorMin.y != child.rectTransform.anchorMax.y )
                    {
                        throw new InvalidOperationException( $"Can't layout in a grid, the child element {c.gameObject.name} fills height." );
                    }
                }

                // Bidirectional layout uses the sizes of child elements. Tracks the maximum size in the current row because that is the width/height of the row.


                float rowSum = 0.0f;
                Vector2 contentsSize = container.contents.GetActualSize();
                float rowLength = FreeAxis == Axis2D.X ? contentsSize.y : contentsSize.x;
                float currentRowPosition = 0.0f;
                float currentRowSize = 0.0f; // width (freeaxis=x) or height (freeaxis=y) of the current row.
                bool currentRowHasElement = false; // true if something was placed in the current row.
                foreach( var child in container.Children )
                {
                    UILayoutInfo layoutInfo = child.rectTransform.GetLayoutInfo();

                    if( DirY == DirectionY.TopToBottom && DirX == DirectionX.LeftToRight )
                    {
                        layoutInfo.anchorMin = Vector2.up;
                        layoutInfo.anchorMax = Vector2.up;
                        layoutInfo.pivot = Vector2.up;
                    }
                    else if( DirY == DirectionY.BottomToTop && DirX == DirectionX.LeftToRight )
                    {
                        layoutInfo.anchorMin = Vector2.zero;
                        layoutInfo.anchorMax = Vector2.zero;
                        layoutInfo.pivot = Vector2.zero;
                    }
                    else if( DirY == DirectionY.TopToBottom && DirX == DirectionX.RightToLeft )
                    {
                        layoutInfo.anchorMin = Vector2.one;
                        layoutInfo.anchorMax = Vector2.one;
                        layoutInfo.pivot = Vector2.one;
                    }
                    else if( DirY == DirectionY.BottomToTop && DirX == DirectionX.RightToLeft )
                    {
                        layoutInfo.anchorMin = Vector2.right;
                        layoutInfo.anchorMax = Vector2.right;
                        layoutInfo.pivot = Vector2.right;
                    }
                    if( FreeAxis == Axis2D.X )
                    {
                        if( (rowSum + layoutInfo.sizeDelta.y) > rowLength && currentRowHasElement )
                        {
                            if( DirX == DirectionX.LeftToRight )
                                currentRowPosition += currentRowSize + Spacing.y;
                            else
                                currentRowPosition -= currentRowSize + Spacing.y;

                            currentRowSize = 0.0f;
                            rowSum = 0.0f;
                            currentRowHasElement = false;
                        }

                        if( DirY == DirectionY.TopToBottom )
                        {
                            layoutInfo.anchoredPosition.y = -rowSum;
                        }
                        else if( DirY == DirectionY.BottomToTop )
                        {
                            layoutInfo.anchoredPosition.y = rowSum;
                        }

                        if( currentRowSize < layoutInfo.sizeDelta.x )
                            currentRowSize = layoutInfo.sizeDelta.x;

                        layoutInfo.anchoredPosition.x = currentRowPosition;

                        currentRowHasElement = true;
                        child.rectTransform.SetLayoutInfo( layoutInfo );

                        rowSum += layoutInfo.sizeDelta.y + Spacing.y; // Y+ towards the top.
                    }
                    else
                    {
                        if( (rowSum + layoutInfo.sizeDelta.x) > rowLength && currentRowHasElement )
                        {
                            Vector2 vaa = container.contents.GetActualSize();
                            if( DirY == DirectionY.TopToBottom )
                                currentRowPosition -= currentRowSize + Spacing.x;
                            else
                                currentRowPosition += currentRowSize + Spacing.x;

                            currentRowSize = 0.0f;
                            rowSum = 0.0f;
                            currentRowHasElement = false;
                        }

                        if( DirX == DirectionX.LeftToRight )
                        {
                            layoutInfo.anchoredPosition.x = rowSum;
                        }
                        else if( DirX == DirectionX.RightToLeft )
                        {
                            layoutInfo.anchoredPosition.x = -rowSum;
                        }

                        if( currentRowSize < layoutInfo.sizeDelta.y )
                            currentRowSize = layoutInfo.sizeDelta.y;

                        layoutInfo.anchoredPosition.y = currentRowPosition;

                        currentRowHasElement = true;
                        child.rectTransform.SetLayoutInfo( layoutInfo );

                        rowSum += layoutInfo.sizeDelta.x + Spacing.x; // Y+ towards the top.
                    }
                }

                if( FitToSize )
                {
                    if( FreeAxis == Axis2D.X && container.contents.anchorMin.x != container.contents.anchorMax.x )
                    {
                        throw new InvalidOperationException( $"Can't fit to size horzontally, the container element {c.gameObject.name} fills width." );
                    }
                    if( FreeAxis == Axis2D.Y && container.contents.anchorMin.y != container.contents.anchorMax.y )
                    {
                        throw new InvalidOperationException( $"Can't fit to size vertically, the container element {c.gameObject.name} fills height." );
                    }

                    if( currentRowPosition != 0 )
                        currentRowPosition = Mathf.Abs( currentRowPosition );

                    container.contents.SetSizeWithCurrentAnchors( (RectTransform.Axis)FreeAxis, currentRowPosition + currentRowSize );
                }
            }
        }
    }
}