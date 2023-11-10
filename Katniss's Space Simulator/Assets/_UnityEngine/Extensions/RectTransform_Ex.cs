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
        /// Calculates the actual size of a rect transform.
        /// </summary>
        public static Vector2 GetActualSize( this RectTransform rt )
        {
            Stack<(Vector2 size, Vector2 anchor)> hierarchyDeltas = new Stack<(Vector2, Vector2)>();

            hierarchyDeltas.Push( (rt.sizeDelta, rt.anchorMax - rt.anchorMin) );

            // Figure out which objects contribute to the actual size and reverse their order (to 'parent then child').
            while( rt.parent != null )
            {
                rt = (RectTransform)rt.parent;
                if( rt.anchorMax != rt.anchorMin ) // if the anchors are equal, then the actual size is always equal to sizeDelta.
                    hierarchyDeltas.Push( (rt.sizeDelta, rt.anchorMax - rt.anchorMin) );
            }

            // Calculate the actual size.
            Vector2 currentSize = Vector2.zero;
            foreach( var delta in hierarchyDeltas )
            {
                currentSize = (currentSize * delta.anchor) + delta.size;
            }

            return currentSize;
        }

        /// <summary>
        /// Sets the screen space position to the value provided by using <see cref="Camera.WorldToScreenPoint(Vector3)"/>.
        /// </summary>
        public static void SetScreenPosition( this RectTransform rt, Vector3 screenSpacePosition, bool hideBehind = true )
        {
            if( screenSpacePosition.z < 0 && hideBehind )
                screenSpacePosition = new Vector3( float.MaxValue, 0.0f, float.MaxValue );
            else
                screenSpacePosition.z = 0.0f; // reset depth.
            rt.position = screenSpacePosition;
        }

        /// <summary>
        /// Sets the screen space position to the world space position viewed by the given camera.
        /// </summary>
        public static void SetScreenPosition( this RectTransform rt, Camera camera, Vector3 worldSpacePosition, bool hideBehind = true )
        {
            Vector3 screenSpacePosition = camera.WorldToScreenPoint( worldSpacePosition, camera.stereoActiveEye );
            if( screenSpacePosition.z < 0 && hideBehind )
                screenSpacePosition = new Vector3( float.MaxValue, 0.0f, float.MaxValue );
            else
                screenSpacePosition.z = 0.0f; // reset depth.
            rt.position = screenSpacePosition;
        }
    }
}