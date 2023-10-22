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
            // RectTransform.sizeDelta is equal to actual size - parent's actual size. If there is no parent, size delta is the actual size.
            // Thus if we sum up these size differences, we get the actual size.

            Vector2 actualSize = rt.sizeDelta;

            // Doesn't matter which way up/down the chain we're going.
            while( rt.parent != null )
            {
                rt = (RectTransform)rt.parent;
                actualSize += rt.sizeDelta;
            }

            return actualSize;
        }
    }
}