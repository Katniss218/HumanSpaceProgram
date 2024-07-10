using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
	public static class MatrixMxN_Ex
	{
		public static MatrixMxN ToMatrixMxN( this Vector2 vec )
		{
			MatrixMxN result = MatrixMxN.ColumnVector( 2 );

			result[0] = vec.x;
			result[1] = vec.y;

			return result;
		}
		
		public static MatrixMxN ToMatrixMxN( this Vector3 vec )
		{
			MatrixMxN result = MatrixMxN.ColumnVector( 3 );

			result[0] = vec.x;
			result[1] = vec.y;
			result[2] = vec.z;

			return result;
		}
		
		public static MatrixMxN ToMatrixMxN( this Vector4 vec )
		{
			MatrixMxN result = MatrixMxN.ColumnVector( 4 );

			result[0] = vec.x;
			result[1] = vec.y;
			result[2] = vec.z;
			result[3] = vec.w;

			return result;
		}
		
		public static MatrixMxN ToMatrixMxN( this Matrix3x3 mat )
		{
			MatrixMxN result = new MatrixMxN( 3, 3 );

			result[0] = mat.m00;
			result[1] = mat.m01;
			result[2] = mat.m02;
			result[3] = mat.m10;
			result[4] = mat.m11;
			result[5] = mat.m12;
			result[6] = mat.m20;
			result[7] = mat.m21;
			result[8] = mat.m22;

			return result;
		}

		public static MatrixMxN ToMatrixMxN( this Matrix4x4 mat )
		{
			MatrixMxN result = new MatrixMxN( 4, 4 );

			result[0] = mat.m00;
			result[1] = mat.m01;
			result[2] = mat.m02;
			result[3] = mat.m03;
			result[4] = mat.m10;
			result[5] = mat.m11;
			result[6] = mat.m12;
			result[7] = mat.m13;
			result[8] = mat.m20;
			result[9] = mat.m21;
			result[10] = mat.m22;
			result[11] = mat.m23;
			result[12] = mat.m30;
			result[13] = mat.m31;
			result[14] = mat.m32;
			result[15] = mat.m33;

			return result;
		}
	}
}