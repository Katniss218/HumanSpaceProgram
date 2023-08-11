using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public static class UIHelper
    {
        // Create UI elements with the specific properties with C#.

        // generalized:

        /*
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
        */
    }
}