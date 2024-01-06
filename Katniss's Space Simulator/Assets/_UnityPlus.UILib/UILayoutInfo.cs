using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib
{
    /// <summary>
    /// A compact way to store and pass UI element layout information.
    /// </summary>
    public struct UILayoutInfo
    {
        public static Vector2 TopLeft = new Vector2( 0.0f, 1.0f );
        public static Vector2 Top = new Vector2( 0.5f, 1.0f );
        public static Vector2 TopRight = new Vector2( 1.0f, 1.0f );

        public static Vector2 Left = new Vector2( 0.0f, 0.5f );
        public static Vector2 Middle = new Vector2( 0.5f, 0.5f );
        public static Vector2 Right = new Vector2( 1.0f, 0.5f );

        public static Vector2 BottomLeft = new Vector2( 0.0f, 0.0f );
        public static Vector2 Bottom = new Vector2( 0.5f, 0.0f );
        public static Vector2 BottomRight = new Vector2( 1.0f, 0.0f );

        public static float LeftF = 0.0f;
        public static float RightF = 1.0f;
        public static float TopF = 1.0f;
        public static float BottomF = 0.0f;

        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;

        /// <summary>
        /// True if the UI element is set to fill the width of its parent.
        /// </summary>
        public bool FillsWidth => (anchorMin.x != anchorMax.x);
        /// <summary>
        /// True if the UI element is set to fill the height of its parent.
        /// </summary>
        public bool FillsHeight => (anchorMin.y != anchorMax.y);

        /// <param name="anchorPivot">The value for anchorMin, anchorMax, and pivot, x: [left..right], y: [bottom..top].</param>
        public UILayoutInfo( Vector2 anchorPivot, Vector2 anchoredPosition, Vector2 sizeDelta )
        {
            this.anchorMin = anchorPivot;
            this.anchorMax = anchorPivot;
            this.pivot = anchorPivot;
            this.anchoredPosition = anchoredPosition;
            this.sizeDelta = sizeDelta;
        }

        public UILayoutInfo( Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta )
        {
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.pivot = new Vector2( (anchorMin.x + anchorMax.x) / 2.0f, (anchorMin.y + anchorMax.y) / 2.0f );
            this.anchoredPosition = anchoredPosition;
            this.sizeDelta = sizeDelta;
        }

        public UILayoutInfo( Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta )
        {
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.pivot = pivot;
            this.anchoredPosition = anchoredPosition;
            this.sizeDelta = sizeDelta;
        }

        /// <summary>
        /// Creates a UI Layout that will fill the entire parent.
        /// </summary>
        public static UILayoutInfo Fill()
        {
            return new UILayoutInfo()
            {
                anchorMin = Vector2.zero,
                anchorMax = Vector2.one,
                pivot = new Vector2( 0.5f, 0.5f ),
                anchoredPosition = Vector2.zero,
                sizeDelta = Vector2.zero
            };
        }

        /// <summary>
        /// Creates a UI Layout that will fill the entire parent, but with specified margins in pixels.
        /// </summary>
        public static UILayoutInfo Fill( float left, float right, float top, float bottom )
        {
            return new UILayoutInfo()
            {
                anchorMin = Vector2.zero,
                anchorMax = Vector2.one,
                pivot = new Vector2( 0.5f, 0.5f ),
                anchoredPosition = new Vector2( (left - right) / 2, (bottom - top) / 2 ),
                sizeDelta = new Vector2( -(left + right), -(top + bottom) )
            };
        }

        /// <summary>
        /// Creates a UI Layout that will fill the entire parent horizontally, with fixed vertical height, and specified margins in pixels.
        /// </summary>
        public static UILayoutInfo FillHorizontal( float left, float right, float anchorPivotY, float posY, float height )
        {
            return new UILayoutInfo()
            {
                anchorMin = new Vector2( 0.0f, anchorPivotY ),
                anchorMax = new Vector2( 1.0f, anchorPivotY ),
                pivot = new Vector2( 0.5f, anchorPivotY ),
                anchoredPosition = new Vector2( (left - right) / 2, posY ),
                sizeDelta = new Vector2( -(left + right), height )
            };
        }

        /// <summary>
        /// Creates a UI Layout that will fill the entire parent vertically, with fixed horizontal width, and specified margins in pixels.
        /// </summary>
        public static UILayoutInfo FillVertical( float top, float bottom, float anchorPivotX, float posX, float width )
        {
            return new UILayoutInfo()
            {
                anchorMin = new Vector2( anchorPivotX, 0.0f ),
                anchorMax = new Vector2( anchorPivotX, 1.0f ),
                pivot = new Vector2( anchorPivotX, 0.5f ),
                anchoredPosition = new Vector2( posX, (bottom - top) / 2 ),
                sizeDelta = new Vector2( width, -(top + bottom) )
            };
        }

        /// <summary>
        /// Creates a UI Layout that will fill the entire parent, but with specified margins as percent of parent's width/height.
        /// </summary>
        public static UILayoutInfo FillPercent( float left, float right, float top, float bottom )
        {
            return new UILayoutInfo()
            {
                anchorMin = new Vector2( left, bottom ),
                anchorMax = new Vector2( 1.0f - right, 1.0f - top ), // since we want to input percent from each edge, we need to invert the max anchor.
                pivot = new Vector2( 0.5f, 0.5f ),
                anchoredPosition = Vector2.zero,
                sizeDelta = Vector2.zero
            };
        }

        /// <summary>
        /// Calculates the actual size of the UI element with the current layout parameters, and a specified parent size.
        /// </summary>
        /// <param name="parentActualSize">The actual size of the parent UI element.</param>
        /// <returns>The width and height of the UI element.</returns>
        public Vector2 GetActualSize( Vector2 parentActualSize )
        {
            if( anchorMin == anchorMax )
            {
                return sizeDelta;
            }

            Vector2 cornerMin = Vector2.Scale( anchorMin, parentActualSize );
            Vector2 cornerMax = Vector2.Scale( anchorMax, parentActualSize );
            Vector2 cornerDelta = cornerMax - cornerMin;
            return cornerDelta + sizeDelta; // With filled width/height, reducing sizeDelta makes the rect smaller. Thus we need to add sizeDelta instead of subtracting.
        }

        /// <summary>
        /// Calculates the actual size of the UI element with the current layout parameters, and a specified parent size.
        /// </summary>
        /// <param name="parentActualSize">The actual size of the parent UI element.</param>
        /// <returns>The width and height of the UI element.</returns>
        public static Vector2 GetActualSize( RectTransform rt )
        {
            if( rt.anchorMin == rt.anchorMax || rt.parent == null )
            {
                return rt.sizeDelta;
            }

            Vector2 parentActualSize = GetActualSize( (RectTransform)rt.parent );

            Vector2 cornerMin = Vector2.Scale( rt.anchorMin, parentActualSize );
            Vector2 cornerMax = Vector2.Scale( rt.anchorMax, parentActualSize );
            Vector2 cornerDelta = cornerMax - cornerMin;
            return cornerDelta + rt.sizeDelta; // With filled width/height, reducing sizeDelta makes the rect smaller. Thus we need to add sizeDelta instead of subtracting.
        }
    }
}