using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// A double-precision <see cref="Quaternion"/>.
    /// </summary>
    public struct QuaternionDbl : IEquatable<Quaternion>, IEquatable<QuaternionDbl>
    {
        const double radToDeg = 180.0 / Math.PI;
        const double degToRad = Math.PI / 180.0;

        /// <summary>
        /// First imaginary coefficient ('b' from 'a + bi + cj + dk').
        /// </summary>
        public double x;

        /// <summary>
        /// Second imaginary coefficient ('c' from 'a + bi + cj + dk').
        /// </summary>
        public double y;

        /// <summary>
        /// Third imaginary coefficient ('d' from 'a + bi + cj + dk').
        /// </summary>
        public double z;

        /// <summary>
        /// The real coefficient of the quaternion ('a' from 'a + bi + cj + dk').
        /// </summary>
        public double w;

        public static QuaternionDbl identity { get => new QuaternionDbl( 0.0, 0.0, 0.0, 1.0 ); }

        public double magnitude { get => (x * x) + (y * y) + (z * z) + (w * w); }

        /// <summary>
        /// Returns the length (norm) of the quaternion.
        /// </summary>
        public double sqrMagnitude { get => Math.Sqrt( magnitude ); }

        /// <summary>
        /// Returns a quaternion with its length (norm) set to 1.
        /// </summary>
        public QuaternionDbl normalized
        {
            get
            {
                double length = this.sqrMagnitude;

                return new QuaternionDbl(
                    x / length,
                    y / length,
                    z / length,
                    w / length );
            }
        }

        public QuaternionDbl( double x, double y, double z, double w )
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        // The dot product between two rotations.
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double Dot( QuaternionDbl a, QuaternionDbl b )
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z) + (a.w * b.w);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl AngleAxis( double angleRadians, Vector3Dbl axis )
        {
            if( axis.sqrMagnitude == 0.0f )
            {
                return QuaternionDbl.identity;
            }

            angleRadians *= 0.5f; // Quaternions use half angles.
            Vector3Dbl axisSin = axis.normalized * Math.Sin( angleRadians );

            return new QuaternionDbl( axisSin.x, axisSin.y, axisSin.z, (float)Math.Cos( angleRadians ) ).normalized;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl Inverse( QuaternionDbl q )
        {
            double lengthSq = q.sqrMagnitude;
            if( lengthSq != 0.0 )
            {
                double i = 1.0 / lengthSq;
                return new QuaternionDbl( q.x * -i, q.y * -i, q.z * -i, q.w * i );
            }
            return q;
        }

        /// <summary>
        /// Returns the angle in degrees between two rotations.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double Angle( QuaternionDbl a, QuaternionDbl b )
        {
            double dot = QuaternionDbl.Dot( a, b );
            return Math.Acos( Math.Min( Math.Abs( dot ), 1f ) ) * 2f * radToDeg;
        }

        /// <summary>
        /// Combines the rotations in order: q1, then q2.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl Multiply( QuaternionDbl lhs, QuaternionDbl rhs ) // works.
        {
            return new QuaternionDbl(
                (lhs.w * rhs.x) + (lhs.x * rhs.w) + (lhs.y * rhs.z) - (lhs.z * rhs.y),
                (lhs.w * rhs.y) + (lhs.y * rhs.w) + (lhs.z * rhs.x) - (lhs.x * rhs.z),
                (lhs.w * rhs.z) + (lhs.z * rhs.w) + (lhs.x * rhs.y) - (lhs.y * rhs.x),
                (lhs.w * rhs.w) - (lhs.x * rhs.x) - (lhs.y * rhs.y) - (lhs.z * rhs.z) ).normalized;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override bool Equals( object other )
        {
            if( other is QuaternionDbl q1 )
                return Equals( q1 );
            if( other is Quaternion q2 )
                return Equals( q2 );

            return false;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Equals( Quaternion other )
        {
            return x.Equals( other.x ) && y.Equals( other.y ) && z.Equals( other.z ) && w.Equals( other.w );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Equals( QuaternionDbl other )
        {
            return x.Equals( other.x ) && y.Equals( other.y ) && z.Equals( other.z ) && w.Equals( other.w );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2) ^ (w.GetHashCode() >> 1);
        }

        public static Vector3Dbl operator *( QuaternionDbl rotation, Vector3Dbl point )
        {
            double x = rotation.x * 2.0;
            double y = rotation.y * 2.0;
            double z = rotation.z * 2.0;
            double xx = rotation.x * x;
            double yy = rotation.y * y;
            double zz = rotation.z * z;
            double xy = rotation.x * y;
            double xz = rotation.x * z;
            double yz = rotation.y * z;
            double wx = rotation.w * x;
            double wy = rotation.w * y;
            double wz = rotation.w * z;

            Vector3Dbl result;
            result.x = (1.0 - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z;
            result.y = (xy + wz) * point.x + (1.0 - (xx + zz)) * point.y + (yz - wx) * point.z;
            result.z = (xz - wy) * point.x + (yz + wx) * point.y + (1.0 - (xx + yy)) * point.z;
            return result;
        }

        public static QuaternionDbl operator *( QuaternionDbl lhs, QuaternionDbl rhs )
        {
            return Multiply( lhs, rhs );
        }

        public static explicit operator Quaternion( QuaternionDbl q )
        {
            return new Quaternion( (float)q.x, (float)q.y, (float)q.z, (float)q.w );
        }

        public static implicit operator QuaternionDbl( Quaternion q )
        {
            return new QuaternionDbl( q.x, q.y, q.z, q.w );
        }
    }
}