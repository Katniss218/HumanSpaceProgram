using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class QuaternionDbl_Ex
    {
        /// <summary>
        /// Returns the `forward` direction for a given orientation.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl GetForwardAxis( this QuaternionDbl q )
        {
            return q * Vector3Dbl.forward;
        }

        /// <summary>
        /// Returns the `right` direction for a given orientation.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl GetRightAxis( this QuaternionDbl q )
        {
            return q * Vector3Dbl.right;
        }

        /// <summary>
        /// Returns the `up` direction for a given orientation.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl GetUpAxis( this QuaternionDbl q )
        {
            return q * Vector3Dbl.up;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl Inverse( this QuaternionDbl q )
        {
            return QuaternionDbl.Inverse( q );
        }
    }
}
