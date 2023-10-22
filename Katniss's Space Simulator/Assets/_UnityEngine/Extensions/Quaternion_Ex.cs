using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Quaternion_Ex
    {
        /// <summary>
        /// Returns the `forward` direction for a given orientation.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetForwardAxis( this Quaternion q )
        {
            return q * Vector3.forward;
        }

        /// <summary>
        /// Returns the `right` direction for a given orientation.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetRightAxis( this Quaternion q )
        {
            return q * Vector3.right;
        }

        /// <summary>
        /// Returns the `up` direction for a given orientation.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetUpAxis( this Quaternion q )
        {
            return q * Vector3.up;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion Inverse( this Quaternion q )
        {
            return Quaternion.Inverse( q );
        }
    }
}
