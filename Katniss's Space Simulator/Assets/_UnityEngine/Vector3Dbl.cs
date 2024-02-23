using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
	/// <summary>
	/// A double-precision <see cref="Vector3"/>.
	/// </summary>
	[Serializable]
	public struct Vector3Dbl : IEquatable<Vector3>, IEquatable<Vector3Dbl>
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
	}
}