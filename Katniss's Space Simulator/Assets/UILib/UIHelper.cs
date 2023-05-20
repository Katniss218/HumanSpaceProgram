using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KatnisssSpaceSimulator.UILib
{
    public static class UIHelper
    {
        // Create UI elements with the specific properties with C#.

        // generalized:

        public static GameObject UI( Transform parent, string name, Vector2 anchorPivot, Vector2 anchoredPos, Vector2 sizeDelta )
        {
            return UI( parent, name, anchorPivot, anchorPivot, anchorPivot, anchoredPos, sizeDelta );
        }

        public static GameObject UI( Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta )
        {
            return UI( parent, name, anchorMin, anchorMax, new Vector2( (anchorMin.x + anchorMax.x) / 2, (anchorMin.y + anchorMax.y) / 2 ), anchoredPos, sizeDelta );
        }

        public static GameObject UI( Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta )
        {
            GameObject gameObject = new GameObject( name );
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.SetParent( parent.transform );
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPos;
            rectTransform.sizeDelta = sizeDelta;

            rectTransform.localScale = Vector3.one;

            return gameObject;
        }

        // fill size relative parent:

        public static GameObject UIFill( Transform parent, string name )
        {
            return UIFill( parent, name, 0, 0, 0, 0 );
        }

        [Obsolete( "The anchored position and size calculations are wrong. Only works for (0,0,0,0)" )]
        static GameObject UIFill( Transform parent, string name, float left, float right, float top, float bottom )
        {
            Vector2 anchorMin = new Vector2( 0.0f, 0.0f );
            Vector2 anchorMax = new Vector2( 1.0f, 1.0f );
            Vector2 pivot = new Vector2( 0.5f, 0.5f );

            Vector2 anchoredPos = new Vector2( left, -top );
            Vector2 sizeDelta = new Vector2( -left - right, -top - bottom );

            return UI( parent, name, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta );
        }

        /// <summary>
        /// Creates a new UI object that fills its parent, and the edges are at the specific percent marks of the parent.
        /// </summary>
        /// <param name="left">Horizontal percent value of the parent.</param>
        /// <param name="right">Horizontal percent value of the parent.</param>
        /// <param name="top">Vertical percent value of the parent.</param>
        /// <param name="bottom">Vertical percent value of the parent.</param>
        public static GameObject UIFillPercent( Transform parent, string name, float left, float right, float top, float bottom )
        {
            Vector2 anchorMin = new Vector2( left, bottom );
            Vector2 anchorMax = new Vector2( 1.0f - right, 1.0f - top );
            Vector2 pivot = new Vector2( 0.5f, 0.5f );
            Vector2 anchoredPos = new Vector2( left, -top );
            Vector2 sizeDelta = new Vector2( -left - right, -top - bottom );

            return UI( parent, name, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta );
        }

        /// <summary>
        /// Makes the UI element a raycast target for the UI event system. Enables the UI object to listen to UI event system inputs.
        /// </summary>
        public static void MakeRaycastTarget( GameObject go )
        {
            Image raycastImage = go.AddComponent<Image>();
            raycastImage.raycastTarget = true;
            raycastImage.color = new Color( 0, 0, 0, 0 ); // transparent.
        }

        /// <summary>
        /// Makes the specific object a vertical layout group.
        /// </summary>
        public static void MakeVerticalLayoutGroup( GameObject go, int padding, int spacing, bool containerFitsContents, bool reversed = false )
        {
            VerticalLayoutGroup vl = go.AddComponent<VerticalLayoutGroup>();
            vl.childAlignment = reversed ? TextAnchor.LowerRight : TextAnchor.UpperLeft;
            vl.padding = new RectOffset( padding, padding, padding, padding );
            vl.spacing = spacing;
            vl.childControlWidth = true;
            vl.childControlHeight = false;
            vl.childScaleWidth = false;
            vl.childScaleHeight = false;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;

            if( containerFitsContents )
            {
                ContentSizeFitter cs = go.AddComponent<ContentSizeFitter>();
                cs.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                cs.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        /// <summary>
        /// Makes the specific object a horizontal layout group.
        /// </summary>
        public static void MakeHorizontalLayoutGroup( GameObject go, int padding, int spacing, bool containerFitsContents, bool reversed = false )
        {
            HorizontalLayoutGroup vl = go.AddComponent<HorizontalLayoutGroup>();
            vl.childAlignment = reversed ? TextAnchor.LowerRight : TextAnchor.UpperLeft;
            vl.padding = new RectOffset( padding, padding, padding, padding );
            vl.spacing = spacing;
            vl.childControlWidth = false;
            vl.childControlHeight = true;
            vl.childScaleWidth = false;
            vl.childScaleHeight = false;
            vl.childForceExpandWidth = false;
            vl.childForceExpandHeight = true;

            if( containerFitsContents )
            {
                ContentSizeFitter cs = go.AddComponent<ContentSizeFitter>();
                cs.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                cs.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        /// <summary>
        /// Makes the specified object into a scroll rect. Adds the required child elements.
        /// </summary>
        /// <remarks>
        /// Doesn't include a scrollbar, doesn't include any layout for the content.
        /// </remarks>
        /// <returns>The gameobject that will contain the contents.</returns>
        public static GameObject AddScrollRect( GameObject obj, bool horizontal, bool vertical )
        {
            GameObject items = UIHelper.UIFill( obj.transform, "items" );

            GameObject viewport = UIHelper.UIFill( items.transform, "viewport" );

            Image maskImage = viewport.AddComponent<Image>();
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = UIHelper.UI( viewport.transform, "content", new Vector2( 0.0f, 1.0f ), new Vector2( 1, 1 ), Vector2.zero, new Vector2( 0.0f, 280.0f ) );

            ScrollRect scrollRect = items.AddComponent<ScrollRect>();
            scrollRect.content = (RectTransform)content.transform;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.5f;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.viewport = (RectTransform)viewport.transform;

            return content;
        }
    }
}