using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class RectTransform_Ex
    {
        /// <summary>
        /// True if the rect transform is set to fill the width of its parent.
        /// </summary>
        public static bool FillsWidth( this RectTransform rt ) => (rt.anchorMin.x != rt.anchorMax.x);

        /// <summary>
        /// True if the rect transform is set to fill the height of its parent.
        /// </summary>
        public static bool FillsHeight( this RectTransform rt ) => (rt.anchorMin.y != rt.anchorMax.y);

        /// <summary>
        /// Gets the center point of the rect transform.
        /// </summary>
        public static Vector2 GetLocalCenter( this RectTransform rectTransform )
        {
            return rectTransform.rect.center;
        }

        /// <summary>
        /// Transforms a point from the local space of <paramref name="from"/>, to the local space of <paramref name="to"/>.
        /// </summary>
        public static Vector2 TransformPointTo( this RectTransform from, Vector2 position, RectTransform to )
        {
            Vector2 canvasSpacePos = from.TransformPoint( position ); // this transforms from/to pivot space.
            return to.InverseTransformPoint( canvasSpacePos );
        }

        /// <summary>
        /// Gets the actual size of a rect transform in canvas space.
        /// </summary>
        public static Vector2 GetActualSize( this RectTransform rectTransform )
        {
            return rectTransform.rect.size;
        }

        /// <summary>
        /// Sets the screen space position to the value provided by using <see cref="Camera.WorldToScreenPoint(Vector3)"/>.
        /// </summary>
        public static void SetScreenPosition( this RectTransform rt, Vector3 screenSpacePosition, bool hideWhenBehindCamera = true )
        {
            if( screenSpacePosition.z < 0 && hideWhenBehindCamera )
                screenSpacePosition = new Vector3( float.MaxValue, 0.0f, float.MaxValue );
            else
                screenSpacePosition.z = 0.0f; // reset depth.
            rt.position = screenSpacePosition;
        }

        /// <summary>
        /// Sets the screen space position to the world space position viewed by the given camera.
        /// </summary>
        public static void SetScreenPosition( this RectTransform rt, Camera camera, Vector3 worldSpacePosition, bool hideWhenBehindCamera = true )
        {
            Vector3 screenSpacePosition = camera.WorldToScreenPoint( worldSpacePosition, camera.stereoActiveEye );
            SetScreenPosition( rt, screenSpacePosition, hideWhenBehindCamera );
        }

        public static void SetVerticalMargins( this RectTransform rectTransform, float top, float bottom )
        {
            rectTransform.sizeDelta = new Vector2( rectTransform.sizeDelta.x, -(top + bottom) );
            rectTransform.anchoredPosition = new Vector2( rectTransform.anchoredPosition.x, (bottom - top) / 2f );
        }

        public static void SetHorizontalMargins( this RectTransform rectTransform, float left, float right )
        {
            rectTransform.sizeDelta = new Vector2( -(left + right), rectTransform.sizeDelta.y );
            rectTransform.anchoredPosition = new Vector2( (left - right) / 2f, rectTransform.anchoredPosition.y );
        }

    }
}