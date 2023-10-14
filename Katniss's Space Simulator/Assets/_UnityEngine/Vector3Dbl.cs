using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// A double-precision Vector3.
    /// </summary>
    [Serializable]
    public struct Vector3Dbl
    {
        [SerializeField]
        public double x;
        [SerializeField]
        public double y;
        [SerializeField]
        public double z;

        public double sqrMagnitude => x * x + y * y + z * z;

        public double magnitude => Math.Sqrt( sqrMagnitude );

        public static readonly Vector3Dbl zero = new Vector3Dbl( 0, 0, 0 );
        public static readonly Vector3Dbl one = new Vector3Dbl( 1, 1, 1 );
        public static readonly Vector3Dbl forward = new Vector3Dbl( 0, 0, 1 );
        public static readonly Vector3Dbl back = new Vector3Dbl( 0, 0, -1 );
        public static readonly Vector3Dbl right = new Vector3Dbl( 1, 0, 0 );
        public static readonly Vector3Dbl left = new Vector3Dbl( -1, 0, 0 );
        public static readonly Vector3Dbl up = new Vector3Dbl( 0, 1, 0 );
        public static readonly Vector3Dbl down = new Vector3Dbl( 0, -1, 0 );

        public Vector3Dbl( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetDirection( Vector3Dbl from, Vector3Dbl to )
        {
            Vector3Dbl dir = to - from;
            dir.Normalize();

            return new Vector3( (float)dir.x, (float)dir.y, (float)dir.z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Normalize()
        {
            double magn = magnitude;
            this.x /= magn;
            this.y /= magn;
            this.z /= magn;
        }

        public Vector3Dbl normalized
        {
            get
            {
                double magn = magnitude;
                return new Vector3Dbl( this.x / magn, this.y / magn, this.z / magn );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Vector3 NormalizeToVector3()
        {
            double magn = magnitude;
            return new Vector3( (float)(this.x / magn), (float)(this.y / magn), (float)(this.z / magn) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double Dot( Vector3Dbl a, Vector3Dbl b )
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Cross( Vector3Dbl v1, Vector3Dbl v2 )
        {
            double x = (v1.y * v2.z) - (v1.z * v2.y);
            double y = (v1.z * v2.x) - (v1.x * v2.z);
            double z = (v1.x * v2.y) - (v1.y * v2.x);

            return new Vector3Dbl( x, y, z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Add( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return new Vector3Dbl( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Add( Vector3Dbl v1, Vector3 v2 )
        {
            return new Vector3Dbl( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Subtract( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return new Vector3Dbl( v1.x - v2.x, v1.y - v2.y, v1.z - v2.z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Subtract( Vector3Dbl v1, Vector3 v2 )
        {
            return new Vector3Dbl( v1.x - v2.x, v1.y - v2.y, v1.z - v2.z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Multiply( Vector3Dbl v, double s )
        {
            return new Vector3Dbl( v.x * s, v.y * s, v.z * s );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Divide( Vector3Dbl v, double s )
        {
            return new Vector3Dbl( v.x / s, v.y / s, v.z / s );
        }

        public static Vector3Dbl operator +( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return Add( v1, v2 );
        }
        public static Vector3Dbl operator +( Vector3Dbl v1, Vector3 v2 )
        {
            return Add( v1, v2 );
        }
        public static Vector3Dbl operator -( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return Subtract( v1, v2 );
        }
        public static Vector3Dbl operator -( Vector3Dbl v1, Vector3 v2 )
        {
            return Subtract( v1, v2 );
        }

        public static Vector3Dbl operator *( Vector3Dbl v, double s )
        {
            return Multiply( v, s );
        }

        public static Vector3Dbl operator *( double s, Vector3Dbl v )
        {
            return Multiply( v, s );
        }

        public static Vector3Dbl operator /( Vector3Dbl v, double s )
        {
            return Divide( v, s );
        }

        public static explicit operator Vector3( Vector3Dbl v )
        {
            return new Vector3( (float)v.x, (float)v.y, (float)v.z );
        }

        public static implicit operator Vector3Dbl( Vector3 v )
        {
            return new Vector3Dbl( v.x, v.y, v.z );
        }
    }
}