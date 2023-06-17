using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    /// <summary>
    /// A compact way to store and pass <see cref="RectTransform"/> layout values.
    /// </summary>
    public struct UILayoutInfo
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;

        /// <param name="anchorPivot">The value for both anchors and pivot.</param>
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
            this.pivot = new Vector2( (anchorMin.x + anchorMax.x) / 2, (anchorMin.y + anchorMax.y) / 2 );
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

        public static UILayoutInfo Fill()
        {
            return new UILayoutInfo()
            {
                anchorMin = Vector2.zero,
                anchorMax = Vector2.one,
                pivot = new Vector2( 0.5f, 0.5f ),
                anchoredPosition = Vector2.zero, //new Vector2( left, -top ),
                sizeDelta = Vector2.zero //new Vector2( -left - right, -top - bottom )
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