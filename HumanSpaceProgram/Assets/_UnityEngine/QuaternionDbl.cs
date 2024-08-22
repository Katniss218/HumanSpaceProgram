using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    /// <summary>
    /// A double-precision <see cref="Quaternion"/>.
    /// </summary>
    public struct QuaternionDbl : IEquatable<Quaternion>, IEquatable<QuaternionDbl>, IFormattable
    {
        const double radToDeg = 180.0 / Math.PI;
        const double degToRad = Math.PI / 180.0;

        /// <summary>
        /// X component of the QuaternionDbl. Don't modify this directly unless you know quaternions inside out.
        /// </summary>
        public double x;

        /// <summary>
        /// Y component of the QuaternionDbl. Don't modify this directly unless you know quaternions inside out.
        /// </summary>
        public double y;

        /// <summary>
        /// Z component of the QuaternionDbl. Don't modify this directly unless you know quaternions inside out.
        /// </summary>
        public double z;

        /// <summary>
        /// W component of the QuaternionDbl. Do not directly modify quaternions.
        /// </summary>
        public double w;

        public double sqrMagnitude { get => (x * x) + (y * y) + (z * z) + (w * w); }

        /// <summary>
        /// Returns the length (norm) of the quaternion.
        /// </summary>
        public double magnitude { get => Math.Sqrt( sqrMagnitude ); }

        /// <summary>
        /// Returns a quaternion with its length (norm) set to 1.
        /// </summary>
        public QuaternionDbl normalized
        {
            get
            {
                double length = this.magnitude;

                return new QuaternionDbl(
                    x / length,
                    y / length,
                    z / length,
                    w / length );
            }
        }

        public static QuaternionDbl identity { get => new QuaternionDbl( 0.0, 0.0, 0.0, 1.0 ); }

        public double this[int index]
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    3 => w,
                    _ => throw new IndexOutOfRangeException( "Invalid QuaternionDbl index!" ),
                };
            }
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            set
            {
                switch( index )
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    case 3:
                        w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException( "Invalid QuaternionDbl index!" );
                }
            }
        }

        public QuaternionDbl( double x, double y, double z, double w )
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// The dot product between two rotations.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double Dot( QuaternionDbl a, QuaternionDbl b )
        {
            return (a.x * b.x) + (a.y * b.y) + (a.z * b.z) + (a.w * b.w);
        }

        /// <summary>
        /// Creates a rotation which rotates angle degrees around axis.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl AngleAxis( double angle, Vector3Dbl axis )
        {
            if( axis.sqrMagnitude == 0.0f )
            {
                return QuaternionDbl.identity;
            }

            angle *= degToRad;
            angle *= 0.5f; // Quaternions use half angles.
            Vector3Dbl axisSin = axis.normalized * Math.Sin( angle );

            return new QuaternionDbl( axisSin.x, axisSin.y, axisSin.z, Math.Cos( angle ) ).normalized;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void ToAngleAxis( out double angle, out Vector3Dbl axis )
        {
            // https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm

            const double epsilon = 0.000001;

            angle = 2.0 * Math.Acos( this.w ); // angle
            double den = Math.Sqrt( 1.0 - (this.w * this.w) );

            angle *= radToDeg;

            if( den < epsilon )
            {
                // This occurs when the angle is zero. Not a problem, just set an arbitrary normalized axis.
                axis = Vector3Dbl.forward;
                return;
            }

            axis = new Vector3Dbl( this.x / den, this.y / den, this.z / den );

        }

        /// <summary>
        /// Returns a rotation that rotates z degrees around the z axis, x degrees around <br/>
        /// the x axis, and y degrees around the y axis; applied in that order.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl Euler( Vector3Dbl euler )
        {
            return Euler( euler.x, euler.y, euler.z );
        }

        /// <summary>
        /// Returns a rotation that rotates z degrees around the z axis, x degrees around <br/>
        /// the x axis, and y degrees around the y axis; applied in that order.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl Euler( double x, double y, double z )
        {
            x *= degToRad;
            y *= degToRad;
            z *= degToRad;

            double cosX = Math.Cos( x * 0.5 );
            double cosY = Math.Cos( y * 0.5 );
            double cosZ = Math.Cos( z * 0.5 );

            double sinX = Math.Sin( x * 0.5 );
            double sinY = Math.Sin( y * 0.5 );
            double sinZ = Math.Sin( z * 0.5 );

            // The 3 terms on the left are opposite to the corresponding terms on the right (sin <-> cos).
            double qx = (cosX * sinY * sinZ) + (sinX * cosY * cosZ); // The order of the terms depends on the desired rotation order and handedness of the system. Here's order for Unity.
            double qy = (cosX * sinY * cosZ) - (sinX * cosY * sinZ);
            double qz = (cosX * cosY * sinZ) - (sinX * sinY * cosZ);
            double qw = (cosX * cosY * cosZ) + (sinX * sinY * sinZ);

            return new QuaternionDbl( qx, qy, qz, qw ); // possibly needs normalizing??
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl LookRotation( Vector3Dbl forward, Vector3Dbl up )
        {
            // https://stackoverflow.com/questions/52413464/look-at-quaternion-using-up-vector
            forward.Normalize();

            // First matrix column
            Vector3Dbl sideAxis = Vector3Dbl.Normalize( Vector3Dbl.Cross( up, forward ) );
            // Second matrix column
            Vector3Dbl rotatedUp = Vector3Dbl.Cross( forward, sideAxis );
            // Third matrix column
            Vector3Dbl lookAt = forward;

            // Sums of matrix main diagonal elements
            double trace1 = 1.0 + sideAxis.x - rotatedUp.y - lookAt.z;
            double trace2 = 1.0 - sideAxis.x + rotatedUp.y - lookAt.z;
            double trace3 = 1.0 - sideAxis.x - rotatedUp.y + lookAt.z;

            // If orthonormal vectors forms identity matrix, then return identity rotation
            if( trace1 + trace2 + trace3 < 0.00000001 )
            {
                return Quaternion.identity;
            }

            // Choose largest diagonal
            if( trace1 + 0.00000001 > trace2 && trace1 + 0.00000001 > trace3 )
            {
                double s = Math.Sqrt( trace1 ) * 2.0;
                return new QuaternionDbl(
                    0.25 * s,
                    (rotatedUp.x + sideAxis.y) / s,
                    (lookAt.x + sideAxis.z) / s,
                    (rotatedUp.z - lookAt.y) / s );
            }
            else if( trace2 + 0.00000001 > trace1 && trace2 + 0.00000001 > trace3 )
            {
                double s = Math.Sqrt( trace2 ) * 2.0;
                return new QuaternionDbl(
                    (rotatedUp.x + sideAxis.y) / s,
                    0.25 * s,
                    (lookAt.y + rotatedUp.z) / s,
                    (lookAt.x - sideAxis.z) / s );
            }
            else
            {
                double s = Math.Sqrt( trace3 ) * 2.0;
                return new QuaternionDbl(
                    (lookAt.x + sideAxis.z) / s,
                    (lookAt.y + rotatedUp.z) / s,
                    0.25 * s,
                    (sideAxis.y - rotatedUp.x) / s );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static QuaternionDbl FromToRotation( Vector3Dbl fromRotation, Vector3Dbl toRotation )
        {
            // https://stackoverflow.com/questions/1171849/finding-quaternion-representing-the-rotation-from-one-vector-to-another
            double kCosTheta = Vector3Dbl.Dot( fromRotation, toRotation );
            double k = Math.Sqrt( fromRotation.sqrMagnitude * toRotation.sqrMagnitude );

            /*if( kCosTheta / k == -1 )
            {
                // 180 degree rotation around any orthogonal vector
                return new QuaternionDbl( 0, Vector3Dbl.Orthogonal( fromRotation ).normalized ).normalized;
            }*/

            Vector3Dbl cross = Vector3Dbl.Cross( fromRotation, toRotation );
            return new QuaternionDbl( cross.x, cross.y, cross.z, kCosTheta + k ).normalized;
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

        public static QuaternionDbl operator *( QuaternionDbl lhs, QuaternionDbl rhs )
        {
            return Multiply( lhs, rhs );
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

        public static explicit operator Quaternion( QuaternionDbl q )
        {
            return new Quaternion( (float)q.x, (float)q.y, (float)q.z, (float)q.w );
        }

        public static implicit operator QuaternionDbl( Quaternion q )
        {
            return new QuaternionDbl( q.x, q.y, q.z, q.w );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static bool IsEqualUsingDot( double dot )
        {
            return dot > 0.999999999;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==( QuaternionDbl lhs, QuaternionDbl rhs )
        {
            return IsEqualUsingDot( Dot( lhs, rhs ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=( QuaternionDbl lhs, QuaternionDbl rhs )
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return ToString( null, null );
        }

        public string ToString( string format )
        {
            return ToString( format, null );
        }

        public string ToString( string format, IFormatProvider formatProvider )
        {
            if( string.IsNullOrEmpty( format ) )
                format = "F5";

            if( formatProvider == null )
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;

            return String.Format( "({0}, {1}, {2}, {3})", x.ToString( format, formatProvider ), y.ToString( format, formatProvider ), z.ToString( format, formatProvider ), w.ToString( format, formatProvider ) );
        }
    }
}