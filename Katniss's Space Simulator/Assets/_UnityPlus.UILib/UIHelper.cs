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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (GameObject go, RectTransform t) CreateUI( UIElement parent, string name, UILayoutInfo layout )
        {
            return CreateUI( parent.transform, name, layout );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static (GameObject go, RectTransform t) CreateUI( RectTransform parent, string name, UILayoutInfo layout )
        {
            GameObject rootGO = new GameObject( name );

            RectTransform rootT = rootGO.AddComponent<RectTransform>();
            rootT.SetParent( parent );
            rootT.SetLayoutInfo( layout );
            rootT.localScale = Vector3.one;

            return (rootGO, rootT);
        }

        /*
        [Obsolete( "The anchored position and size calculations are wrong. Only works for (0,0,0,0)" )]
        public static GameObject UIFill( Transform parent, string name, float left, float right, float top, float bottom )
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
            Image raycastImage = go.GetComponent<Image>();
            if( raycastImage == null )
            {
                raycastImage = go.AddComponent<Image>(); // Image is required to register raycasts without a custom component.
                raycastImage.color = new Color( 0, 0, 0, 0 ); // Setting alpha to 0 makes image invisible.
            }
            raycastImage.raycastTarget = true;
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