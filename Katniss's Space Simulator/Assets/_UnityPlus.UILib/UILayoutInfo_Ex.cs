using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib;

namespace UnityPlus.UILib
{
    public static class UILayoutInfo_Ex
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
