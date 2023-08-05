using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib
{
    /// <summary>
    /// A compact way to store and pass <see cref="RectTransform"/> layout information.
    /// </summary>
    public struct UILayoutInfo
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;

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
    }

    public static class LayoutInfoEx
    {
        /// <summary>
        /// Sets the layout properties of this Rect Transform to the specified values.
        /// </summary>
        public static void SetLayoutInfo( this RectTransform transform, UILayoutInfo layoutInfo )
        {
            transform.anchorMin = layoutInfo.anchorMin;
            transform.anchorMax = layoutInfo.anchorMax;
            transform.pivot = layoutInfo.pivot;
            transform.anchoredPosition = layoutInfo.anchoredPosition;
            transform.sizeDelta = layoutInfo.sizeDelta;
        }

        /// <summary>
        /// Gets the layout properties of this Rect Transform.
        /// </summary>
        public static UILayoutInfo GetLayoutInfo( this RectTransform transform )
        {
            return new UILayoutInfo()
            {
                anchorMin = transform.anchorMin,
                anchorMax = transform.anchorMax,
                pivot = transform.pivot,
                anchoredPosition = transform.anchoredPosition,
                sizeDelta = transform.sizeDelta
            };
        }
    }
}