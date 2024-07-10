using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Vector3Dbl_Ex
    {
        /// <summary>
        /// Returns the axis vector for the axis with the largest component. The main axis of a vector.
        /// </summary>
        public static Vector3Dbl GetPrincipalAxis( this Vector3Dbl v )
        {
            Vector3Dbl newV = v;
            double absX = Math.Abs( v.x );
            double absY = Math.Abs( v.y );
            double absZ = Math.Abs( v.z );

            if( absX < absY || absX < absZ ) // x smaller than both other axes.
            {
                newV.x = 0.0;
            }
            if( absY < absX || absY < absZ ) // y smaller than both other axes.
            {
                newV.y = 0.0;
            }
            if( absZ < absX || absZ < absY ) // z smaller than both other axes.
            {
                newV.z = 0.0;
            }

            return newV;
        }
    }
}