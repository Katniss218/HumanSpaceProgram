using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
	public static class Matrix3x3_Ex
	{
		public static (Vector3 eigenvector, float eigenvalue)[] QRAlgorithm( this Matrix3x3 A )
		{
			throw new NotImplementedException();
		}
		public static float GetMagnitude( float[] vector )
		{
			float acc = 0;
			for( int i = 0; i < vector.Length; i++ )
			{
				var val = vector[i];
				acc += val * val;
			}
			return Mathf.Sqrt( acc );
		}

		static Matrix3x3 MultiplyRowAndCol( float[] rowVector, float[] colVector )
		{
			if( rowVector.Length != 3 || colVector.Length != 3 )
			{
				throw new ArgumentException( "Both vectors must have a length of 3." );
			}

			Matrix3x3 resultMatrix = Matrix3x3.zero;

			for( int i = 0; i < 3; i++ )
			{
				for( int j = 0; j < 3; j++ )
				{
					resultMatrix[i, j] = rowVector[i] * colVector[j];
				}
			}

			return resultMatrix;
		}

		[Obsolete( "untested" )]
		public static (Matrix3x3 Q, Matrix3x3 R) MatDecomposeQR( Matrix3x3 mat )
		{
			// https://visualstudiomagazine.com/Articles/2024/01/03/matrix-inverse.aspx
			// QR decomposition, Householder algorithm.

			int rows = 3;
			int cols = 3;

			Matrix3x3 Q = Matrix3x3.identity;
			Matrix3x3 R = Matrix3x3.zero;

			for( int currentCol = 0; currentCol < cols - 1; currentCol++ )
			{
				Matrix3x3 H = Matrix3x3.identity;
				float[] a = new float[cols - currentCol];
				int k = 0;
				for( int row = currentCol; row < cols; row++ )
				{
					a[k] = R[row, currentCol];
					k++;
				}

				float aMagnitude = GetMagnitude( a );
				if( a[0] < 0.0 )
				{
					aMagnitude = -aMagnitude;
				}

				Vector3 v = Vector3.zero;
				for( k = 0; k < 3; k++ )
				{
					v[k] = a[k] / (a[0] + aMagnitude);
				}
				v[0] = 1;

				float[] alphaRow = new float[] { v.x, v.y, v.z };
				float[] betaCol = new float[] { v.x, v.y, v.z };
				Matrix3x3 aMultB = MultiplyRowAndCol( alphaRow, betaCol ); // matrix-multiply lhs (row vector) with rhs (column vector)

				float[,] h = new float[a.Length, a.Length];
				for( int i = 0; i < a.Length; i++ )
				{
					h[i, i] = 1.0f;
				}

				for( int row = 0; row < 3; row++ )
				{
					for( int col = 0; col < 3; col++ )
					{
						h[row, col] -= (2f / v.sqrMagnitude) * aMultB[row, col];
					}
				}

				// copy h into lower right of H
				int d = cols - a.Length;
				for( int row = 0; row < 3; row++ )
				{
					for( int col = 0; col < 3; col++ )
					{
						H[row + d, col + d] = h[row, col];
					}
				}

				Q = Matrix3x3.Multiply( Q, H );
				R = Matrix3x3.Multiply( H, R );
			}

			return (Q, R);
		}
	}
}