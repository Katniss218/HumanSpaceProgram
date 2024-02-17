using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
	public struct Matrix3x3
	{
		public float m00, m01, m02,
				 	 m10, m11, m12,
				  	 m20, m21, m22;

		private static readonly Matrix3x3 zeroMatrix = new Matrix3x3( 0, 0, 0, 0, 0, 0, 0, 0, 0 );

		private static readonly Matrix3x3 identityMatrix = new Matrix3x3( 1, 0, 0, 0, 1, 0, 0, 0, 1 );

		public static Matrix3x3 zero => zeroMatrix;
		public static Matrix3x3 identity => identityMatrix;

		public Matrix3x3( float m00, float m01, float m02,
						  float m10, float m11, float m12,
						  float m20, float m21, float m22 )
		{
			this.m00 = m00; this.m01 = m01; this.m02 = m02;
			this.m10 = m10; this.m11 = m11; this.m12 = m12;
			this.m20 = m20; this.m21 = m21; this.m22 = m22;
		}

		public float this[int row, int column]
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get
			{
				return this[row + column * 3];
			}
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set
			{
				this[row + column * 3] = value;
			}
		}
		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => m00,
					1 => m10,
					2 => m20,
					3 => m01,
					4 => m11,
					5 => m21,
					6 => m02,
					7 => m12,
					8 => m22,
					_ => throw new IndexOutOfRangeException( "Invalid matrix index!" ),
				};
			}
			set
			{
				switch( index )
				{
					case 0:
						m00 = value; break;
					case 1:
						m10 = value; break;
					case 2:
						m20 = value; break;
					case 3:
						m01 = value; break;
					case 4:
						m11 = value; break;
					case 5:
						m21 = value; break;
					case 6:
						m02 = value; break;
					case 7:
						m12 = value; break;
					case 8:
						m22 = value; break;
					default:
						throw new IndexOutOfRangeException( "Invalid matrix index!" );
				}
			}
		}

		//public Matrix3x3 inverse;

		public Matrix3x3 transpose => Transpose( this );

		public static Matrix3x3 Transpose( Matrix3x3 matrix )
		{
			return new Matrix3x3(
				matrix.m00, matrix.m10, matrix.m20,
				matrix.m01, matrix.m11, matrix.m21,
				matrix.m02, matrix.m12, matrix.m22 );
		}

		public static Matrix3x3 Multiply( Matrix3x3 lhs, Matrix3x3 rhs )
		{
			Matrix3x3 result = new Matrix3x3();

			result.m00 = (lhs.m00 * rhs.m00) + (lhs.m01 * rhs.m10) + (lhs.m02 * rhs.m20);
			result.m01 = (lhs.m00 * rhs.m01) + (lhs.m01 * rhs.m11) + (lhs.m02 * rhs.m21);
			result.m02 = (lhs.m00 * rhs.m02) + (lhs.m01 * rhs.m12) + (lhs.m02 * rhs.m22);

			result.m10 = (lhs.m10 * rhs.m00) + (lhs.m11 * rhs.m10) + (lhs.m12 * rhs.m20);
			result.m11 = (lhs.m10 * rhs.m01) + (lhs.m11 * rhs.m11) + (lhs.m12 * rhs.m21);
			result.m12 = (lhs.m10 * rhs.m02) + (lhs.m11 * rhs.m12) + (lhs.m12 * rhs.m22);

			result.m20 = (lhs.m20 * rhs.m00) + (lhs.m21 * rhs.m10) + (lhs.m22 * rhs.m20);
			result.m21 = (lhs.m20 * rhs.m01) + (lhs.m21 * rhs.m11) + (lhs.m22 * rhs.m21);
			result.m22 = (lhs.m20 * rhs.m02) + (lhs.m21 * rhs.m12) + (lhs.m22 * rhs.m22);

			return result;
		}

		public static Matrix3x3 Multiply( Matrix3x3 m, float s )
		{
			Matrix3x3 result = new Matrix3x3();

			result.m00 = m.m00 * s;
			result.m01 = m.m01 * s;
			result.m02 = m.m02 * s;
			result.m10 = m.m10 * s;
			result.m11 = m.m11 * s;
			result.m12 = m.m12 * s;
			result.m20 = m.m20 * s;
			result.m21 = m.m21 * s;
			result.m22 = m.m22 * s;

			return result;
		}

		public static Matrix3x3 operator *( Matrix3x3 m, float s )
		{
			return Multiply( m, s );
		}
		public static Matrix3x3 operator *( float s, Matrix3x3 m )
		{
			return Multiply( m, s );
		}

		public static Matrix3x3 operator *( Matrix3x3 lhs, Matrix3x3 rhs )
		{
			return Multiply( lhs, rhs );
		}

		public static Matrix3x3 operator -( Matrix3x3 lhs, Matrix3x3 rhs )
		{
			Matrix3x3 result = new Matrix3x3();

			result.m00 = lhs.m00 - rhs.m00;
			result.m01 = lhs.m01 - rhs.m01;
			result.m02 = lhs.m02 - rhs.m02;
			result.m10 = lhs.m10 - rhs.m10;
			result.m11 = lhs.m11 - rhs.m11;
			result.m12 = lhs.m12 - rhs.m12;
			result.m20 = lhs.m20 - rhs.m20;
			result.m21 = lhs.m21 - rhs.m21;
			result.m22 = lhs.m22 - rhs.m22;

			return result;
		}

		/// <summary>
		/// Creates a rotation matrix.
		/// </summary>
		public static Matrix3x3 Rotate( Quaternion q )
		{
			float x2 = q.x * 2f;
			float y2 = q.y * 2f;
			float z2 = q.z * 2f;
			float xx2 = q.x * x2;
			float yy2 = q.y * y2;
			float zz2 = q.z * z2;
			float xy2 = q.x * y2;
			float xz2 = q.x * z2;
			float yz2 = q.y * z2;
			float wx2 = q.w * x2;
			float wy2 = q.w * y2;
			float wz2 = q.w * z2;
			Matrix3x3 result = new Matrix3x3( 1.0f - (yy2 + zz2),
											  xy2 - wz2,
											  xz2 + wy2,
											  xy2 + wz2,
											  1.0f - (xx2 + zz2),
											  yz2 - wx2,
											  xz2 - wy2,
											  yz2 + wx2,
											  1.0f - (xx2 + yy2) );
			return result;
		}

		public static Matrix3x3 Scale( Vector3 vector )
		{
			return new Matrix3x3( vector.x, 0f, 0f, 0f, vector.y, 0f, 0f, 0f, vector.z );
		}

		public Quaternion rotation => GetRotation();

		private Quaternion GetRotation()
		{
			float trace = m00 + m11 + m22; // I removed + 1.0f; see discussion with Ethan
			if( trace > 0 )
			{
				float s = 0.5f / Mathf.Sqrt( trace + 1.0f );
				return new Quaternion( (m21 - m12) * s,
									   (m02 - m20) * s,
									   (m10 - m01) * s,
									   0.25f / s );
			}
			else
			{
				if( m00 > m11 && m00 > m22 )
				{
					float s = 2.0f * Mathf.Sqrt( 1.0f + m00 - m11 - m22 );
					return new Quaternion( 0.25f * s,
										   (m01 + m10) / s,
										   (m02 + m20) / s,
										   (m21 - m12) / s );
				}
				else if( m11 > m22 )
				{
					float s = 2.0f * Mathf.Sqrt( 1.0f + m11 - m00 - m22 );
					return new Quaternion( (m01 + m10) / s,
										   0.25f * s,
										   (m12 + m21) / s,
										   (m02 - m20) / s );
				}
				else
				{
					float s = 2.0f * Mathf.Sqrt( 1.0f + m22 - m00 - m11 );
					return new Quaternion( (m02 + m20) / s,
										   (m12 + m21) / s,
										   0.25f * s,
										   (m10 - m01) / s );
				}
			}
		}

		public static bool operator ==( Matrix3x3 lhs, Matrix3x3 rhs )
		{
			float epsilon = 1e-6f;

			return Mathf.Abs( lhs.m00 - rhs.m00 ) < epsilon &&
				   Mathf.Abs( lhs.m01 - rhs.m01 ) < epsilon &&
				   Mathf.Abs( lhs.m02 - rhs.m02 ) < epsilon &&
				   Mathf.Abs( lhs.m10 - rhs.m10 ) < epsilon &&
				   Mathf.Abs( lhs.m11 - rhs.m11 ) < epsilon &&
				   Mathf.Abs( lhs.m12 - rhs.m12 ) < epsilon &&
				   Mathf.Abs( lhs.m20 - rhs.m20 ) < epsilon &&
				   Mathf.Abs( lhs.m21 - rhs.m21 ) < epsilon &&
				   Mathf.Abs( lhs.m22 - rhs.m22 ) < epsilon;
		}

		public static bool operator !=( Matrix3x3 lhs, Matrix3x3 rhs )
		{
			return !(lhs == rhs);
		}
	}
}