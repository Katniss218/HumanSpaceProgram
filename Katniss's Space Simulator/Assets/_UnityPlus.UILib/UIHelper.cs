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