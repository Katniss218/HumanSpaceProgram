using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class QuaternionEx
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetForwardAxis( this Quaternion q )
        {
            return q * Vector3.forward;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetRightAxis( this Quaternion q )
        {
            return q * Vector3.right;
        }

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
