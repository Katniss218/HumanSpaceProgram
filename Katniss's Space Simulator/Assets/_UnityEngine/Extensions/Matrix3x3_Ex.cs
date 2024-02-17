using System;

namespace UnityEngine
{
	public static class Matrix3x3_Ex
	{
		/// <summary>
		/// Returns an (unordered) set of eigenvectors and eigenvalues for the specified matrix. <br/>
		/// The returned eigenvectors are normalized.
		/// </summary>
		public static (Vector3 eigenvector, float eigenvalue)[] Diagonalize( this Matrix3x3 matrix )
		{
			// Adapted from here:
			// https://github.com/thversfelt/SimpleQRAlgorithm/blob/main/QRAlgorithm.cs

			Matrix3x3 currentMatrix = matrix;
			Matrix3x3 eigenMatrix = Matrix3x3.identity;

			// QR iterations.
			for( int i = 0; i < 1000; i++ )
			{
				(Matrix3x3 Q, Matrix3x3 R) = QRDecomposition( currentMatrix );
				currentMatrix = R * Q;
				eigenMatrix = eigenMatrix * Q;

				// Some eigenvalues sometimes switch sign, because the Q ends up with negative values.
				// But when ignoring the sign, they are correct.
				if( currentMatrix.IsDiagonal() )
				{
					break;
				}
			}

			return new (Vector3 eigenvector, float eigenvalue)[]
			{
				(new Vector3( eigenMatrix.m00, eigenMatrix.m10, eigenMatrix.m20 ).normalized, currentMatrix.m00),
				(new Vector3( eigenMatrix.m01, eigenMatrix.m11, eigenMatrix.m21 ).normalized, currentMatrix.m11),
				(new Vector3( eigenMatrix.m02, eigenMatrix.m12, eigenMatrix.m22 ).normalized, currentMatrix.m22)
			};
		}

		/// <summary>
		/// Performs QR decomposition of the specified matrix.
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public static (Matrix3x3 Q, Matrix3x3 R) QRDecomposition( this Matrix3x3 matrix )
		{
			// QR decomposition, Householder algorithm.
			// Adapted from here:
			// https://visualstudiomagazine.com/Articles/2024/01/03/matrix-inverse.aspx

			int rows = 3;
			int cols = 3;

			Matrix3x3 Q = Matrix3x3.identity;
			Matrix3x3 R = matrix;

			for( int currentCol = 0; currentCol < cols - 1; currentCol++ )
			{
				Matrix3x3 H = Matrix3x3.identity;
				MatrixMxN a = MatrixMxN.ColumnVector( cols - currentCol );
				int k = 0;
				for( int row = currentCol; row < cols; row++ )
				{
					a[k] = R[row, currentCol];
					k++;
				}

				float aMagnitude = a.magnitude;
				if( a[0] < 0.0 )
				{
					aMagnitude = -aMagnitude;
				}

				MatrixMxN v = MatrixMxN.ColumnVector( a.Rows );
				for( k = 0; k < v.Rows; k++ )
				{
					v[k] = a[k] / (a[0] + aMagnitude);
				}
				v[0] = 1;

				MatrixMxN alphaCol = MatrixMxN.ColumnVector( a.Rows );
				MatrixMxN betaRow = MatrixMxN.RowVector( a.Rows );
				for( k = 0; k < v.Rows; k++ )
				{
					alphaCol[k] = v[k];
					betaRow[k] = v[k];
				}

				MatrixMxN aMultB = MatrixMxN.Multiply( alphaCol, betaRow );

				float[,] h = new float[a.Rows, a.Rows];
				for( int i = 0; i < a.Rows; i++ )
				{
					h[i, i] = 1.0f;
				}

				for( int row = 0; row < a.Rows; row++ )
				{
					for( int col = 0; col < a.Rows; col++ )
					{
						h[row, col] -= (2f / v.sqrMagnitude) * aMultB[row, col];
					}
				}

				// copy h into lower right of H
				int d = cols - a.Rows;
				for( int row = 0; row < a.Rows; row++ )
				{
					for( int col = 0; col < a.Rows; col++ )
					{
						H[row + d, col + d] = h[row, col];
					}
				}

				Q = Matrix3x3.Multiply( Q, H );
				R = Matrix3x3.Multiply( H, R );
			}

			return (Q, R);
		}

		public static bool IsDiagonal( this Matrix3x3 mat)
		{
			const float epsilon = 1e-6f;

			return Mathf.Abs( mat.m01 ) < epsilon &&
				   Mathf.Abs( mat.m02 ) < epsilon &&
				   Mathf.Abs( mat.m10 ) < epsilon &&
				   Mathf.Abs( mat.m12 ) < epsilon &&
				   Mathf.Abs( mat.m20 ) < epsilon &&
				   Mathf.Abs( mat.m21 ) < epsilon;
		}
	}
}