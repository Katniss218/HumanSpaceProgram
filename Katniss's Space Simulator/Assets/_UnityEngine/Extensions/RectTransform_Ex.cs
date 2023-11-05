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
    }
}