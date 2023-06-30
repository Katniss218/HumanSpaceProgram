using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Vector3_Ex
    {
        /// <summary>
        /// Returns the axis vector for the axis with the largest component. The main axis of a vector.
        /// </summary>
        public static Vector3 GetPrincipalAxis( this Vector3 v )
        {
            float absX = Mathf.Abs( v.x );
            float absY = Mathf.Abs( v.y );
            float absZ = Mathf.Abs( v.z );

            if( absX < absY || absX < absZ )
            {
                v.x = 0.0f;
            }
            if( absY < absX || absY < absZ )
            {
                v.y = 0.0f;
            }
            if( absZ < absX || absZ < absY )
            {
                v.z = 0.0f;
            }

            return v;
        }
    }
}