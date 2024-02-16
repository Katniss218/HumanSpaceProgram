using System;
using System.Collections.Generic;
using System.Linq;
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

			result.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20;
			result.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21;
			result.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22;

			result.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20;
			result.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21;
			result.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22;

			result.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20;
			result.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21;
			result.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22;

			return result;
		}
	}
}