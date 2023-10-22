using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Vector2_Ex
    {
        /// <summary>
        /// Returns the axis vector for the axis with the largest component. The main axis of a vector.
        /// </summary>
        public static Vector2 GetPrincipalAxis( this Vector2 v )
        {
            Vector2 newV = v;
            float absX = Mathf.Abs( v.x );
            float absY = Mathf.Abs( v.y );

            // Zero out the axes that aren't the max.
            // Leave the vector if it doesn't have the principal axis.
            if( absX < absY )
            {
                newV.x = 0.0f;
            }
            if( absY < absX )
            {
                newV.y = 0.0f;
            }

            return newV;
        }
    }
}