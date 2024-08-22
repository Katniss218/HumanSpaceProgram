using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    /// <summary>
    /// A double-precision <see cref="Vector3"/>.
    /// </summary>
    [Serializable]
	public struct Vector3Dbl : IEquatable<Vector3>, IEquatable<Vector3Dbl>, IFormattable
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

		public double this[int index]
		{
			get
			{
				return index switch
				{
					0 => x,
					1 => y,
					2 => z,
					_ => throw new IndexOutOfRangeException( $"Invalid {nameof( Vector3Dbl )} index!" ),
				};
			}
			set
			{
				switch( index )
				{
					case 0:
						x = value; break;
					case 1:
						y = value; break;
					case 2:
						z = value; break;
					default:
						throw new IndexOutOfRangeException( $"Invalid {nameof( Vector3Dbl )} index!" );
				}
			}
		}

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
		public static Vector3Dbl Normalize( Vector3Dbl value )
		{
			double magn = value.magnitude;
			return new Vector3Dbl( value.x / magn, value.y / magn, value.z / magn );
		}
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector3 NormalizeToVector3()
		{
			double magn = magnitude;
			return new Vector3( (float)(this.x / magn), (float)(this.y / magn), (float)(this.z / magn) );
		}

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void OrthoNormalize( ref Vector3Dbl normal, ref Vector3Dbl tangent )
		{
			normal.Normalize();
			tangent = ProjectOnPlane( tangent, normal ).normalized;
		}

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void OrthoNormalize( ref Vector3Dbl normal, ref Vector3Dbl tangent, ref Vector3Dbl binormal )
		{
			normal.Normalize();
			tangent = ProjectOnPlane( tangent, normal ).normalized;
			binormal = ProjectOnPlane( ProjectOnPlane( binormal, tangent ), normal ).normalized;
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
		public static double Distance( Vector3Dbl v1, Vector3Dbl v2 )
		{
			return (v1 - v2).magnitude;
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

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Scale( Vector3Dbl other )
		{
			x *= other.x;
			y *= other.y;
			z *= other.z;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Scale( Vector3 other )
		{
			x *= other.x;
			y *= other.y;
			z *= other.z;
		}

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Project( Vector3Dbl vector, Vector3Dbl onNormal )
        {
            double normalSqrMag = onNormal.sqrMagnitude;
            if( normalSqrMag < double.Epsilon )
                return zero;

            double dot = Dot( vector, onNormal );
            return new Vector3Dbl( onNormal.x * dot / normalSqrMag, onNormal.y * dot / normalSqrMag, onNormal.z * dot / normalSqrMag );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl ProjectOnPlane( Vector3Dbl vector, Vector3Dbl planeNormal )
        {
            double normalSqrMag = planeNormal.sqrMagnitude;
            if( normalSqrMag < double.Epsilon )
                return vector;

            double dot = Dot( vector, planeNormal );
            return new Vector3Dbl( vector.x - planeNormal.x * dot / normalSqrMag, vector.y - planeNormal.y * dot / normalSqrMag, vector.z - planeNormal.z * dot / normalSqrMag );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Reflect( Vector3Dbl inDirection, Vector3Dbl inNormal )
        {
            double minusTwoDot = -2f * Dot( inNormal, inDirection );

            return new Vector3Dbl( minusTwoDot * inNormal.x + inDirection.x, minusTwoDot * inNormal.y + inDirection.y, minusTwoDot * inNormal.z + inDirection.z );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double Angle( Vector3Dbl from, Vector3Dbl to )
        {
            double num = (double)Math.Sqrt( from.sqrMagnitude * to.sqrMagnitude );
            if( num < 1E-15 )
            {
                return 0f;
            }

            double num2 = Math.Clamp( Dot( from, to ) / num, -1f, 1f );
            return (double)Math.Acos( num2 ) * 57.29578f;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double SignedAngle( Vector3Dbl from, Vector3Dbl to, Vector3Dbl axis )
        {
            double angle = Angle( from, to );
            double crossX = (from.y * to.z) - (from.z * to.y);
            double crossY = (from.z * to.x) - (from.x * to.z);
            double crossZ = (from.x * to.y) - (from.y * to.x);
            double sign = Math.Sign( (axis.x * crossX) + (axis.y * crossY) + (axis.z * crossZ) );
            return angle * sign;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Lerp( Vector3Dbl a, Vector3Dbl b, double t )
        {
            t = Math.Clamp( t, 0, 1 );
            return new Vector3Dbl( a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl LerpUnclamped( Vector3Dbl a, Vector3Dbl b, double t )
        {
            return new Vector3Dbl( a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl Slerp( Vector3Dbl a, Vector3Dbl b, double t )
        {
            t = Math.Clamp( t, 0, 1 );

            double dot = Math.Clamp( Dot( a, b ), -1.0f, 1.0f );

            // calculate the angle between the start/end vectors, and multiply by how far along that angle we want to interpolate to get the angle between the start and the returned interpolated vector.
            double angle = (double)Math.Acos( dot ) * t;

            // Calculate the interpolated vector using a formula based on the angle and the start and end vectors.
            Vector3Dbl direction = (b - a * dot).normalized;

            return ((a * Math.Cos( angle )) + (direction * Math.Sin( angle ))).normalized;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl SlerpUnclamped( Vector3Dbl a, Vector3Dbl b, double t )
        {
            double dot = Math.Clamp( Dot( a, b ), -1.0f, 1.0f );

            // calculate the angle between the start/end vectors, and multiply by how far along that angle we want to interpolate to get the angle between the start and the returned interpolated vector.
            double angle = (double)Math.Acos( dot ) * t;

            // Calculate the interpolated vector using a formula based on the angle and the start and end vectors.
            Vector3Dbl direction = (b - a * dot).normalized;

            return ((a * Math.Cos( angle )) + (direction * Math.Sin( angle ))).normalized;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Equals( object other )
		{
			if( other is Vector3Dbl v1 )
				return Equals( v1 );
			if( other is Vector3 v2 )
				return Equals( v2 );

			return false;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Equals( Vector3 other )
		{
			return x == other.x && y == other.y && z == other.z;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Equals( Vector3Dbl other )
		{
			return x == other.x && y == other.y && z == other.z;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
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
		
		public static Vector3Dbl operator -( Vector3Dbl a )
		{
			return new Vector3Dbl( 0f - a.x, 0f - a.y, 0f - a.z );
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

		public static bool operator ==( Vector3Dbl lhs, Vector3Dbl rhs )
		{
			return Math.Abs( lhs.x - rhs.x ) < 1e-12
				&& Math.Abs( lhs.y - rhs.y ) < 1e-12
				&& Math.Abs( lhs.z - rhs.z ) < 1e-12;
		}
		public static bool operator !=( Vector3Dbl lhs, Vector3Dbl rhs )
		{
			return Math.Abs( lhs.x - rhs.x ) >= 1e-12
				|| Math.Abs( lhs.y - rhs.y ) >= 1e-12
				|| Math.Abs( lhs.z - rhs.z ) >= 1e-12;
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
                format = "F2";

            if( formatProvider == null )
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;

            return String.Format( "({0}, {1}, {2})", x.ToString( format, formatProvider ), y.ToString( format, formatProvider ), z.ToString( format, formatProvider ) );
        }
    }
}