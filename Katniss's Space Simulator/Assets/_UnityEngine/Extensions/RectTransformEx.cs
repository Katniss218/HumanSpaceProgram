using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class RectTransformEx
    {
        /// <summary>
        /// Calculates the actual size of a rect transform.
        /// </summary>
        public static Vector2 GetActualSize( this RectTransform rt )
        {
            Vector2 actualSize = rt.sizeDelta;

            while( rt.parent != null )
            {
                rt = (RectTransform)rt.parent;
                actualSize += rt.sizeDelta;
            }

            return actualSize;
        }
    }
}