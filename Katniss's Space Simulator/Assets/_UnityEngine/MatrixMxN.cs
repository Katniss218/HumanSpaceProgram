using System;

namespace UnityEngine
{
	/// <summary>
	/// An arbitrary-size matrix with M rows and N columns. <br/>
	/// Also an arbitrary-size row or column vector.
	/// </summary>
	public struct MatrixMxN
	{
		private float[] _data;

		public int Rows { get; private set; }
		public int Cols { get; private set; }

		/// <summary>
		/// Creates a matrix of given size containing only zeros.
		/// </summary>
		public MatrixMxN( int rows, int cols )
		{
			if( rows < 1 )
			{
				throw new ArgumentException( $"Tried to create a {nameof( MatrixMxN )} with {rows} rows. A {nameof( MatrixMxN )} must have at least 1 row." );
			}
			if( cols < 1 )
			{
				throw new ArgumentException( $"Tried to create a {nameof( MatrixMxN )} with {cols} columns. A {nameof( MatrixMxN )} must have at least 1 column." );
			}

			_data = new float[rows * cols];
			Rows = rows;
			Cols = cols;
		}

		public float this[int i]
		{
			get { return _data[i]; }
			set { _data[i] = value; }
		}
		
		public float this[int row, int col]
		{
			get { return _data[row * this.Cols + col]; }
			set { _data[row * this.Cols + col] = value; }
		}

		/// <summary>
		/// Creates a zero matrix with the specified number of rows and columns.
		/// </summary>
		public static MatrixMxN Zero( int rows, int cols )
		{
			MatrixMxN mat = new();

			mat._data = new float[rows * cols];
			mat.Rows = rows;
			mat.Cols = cols;

			return mat;
		}
		
		/// <summary>
		/// Creates a square identity matrix with the specified number of rows and columns.
		/// </summary>
		public static MatrixMxN Identity( int size )
		{
			MatrixMxN mat = new();

			mat._data = new float[size * size];
			mat.Rows = size;
			mat.Cols = size;

			for( int i = 0; i < size; i++ )
			{
				mat._data[i] = 1;
			}

			return mat;
		}

		/// <summary>
		/// Gets the square of the magnitude of the row or column vector.
		/// </summary>
		public float sqrMagnitude
		{
			get
			{
				float acc = 0;
				for( int i = 0; i < _data.Length; i++ )
				{
					var val = _data[i];
					acc += val * val;
				}
				return acc;
			}
		}
		
		/// <summary>
		/// Gets the the magnitude of the row or column vector.
		/// </summary>
		public float magnitude
		{
			get
			{
				return Mathf.Sqrt( sqrMagnitude );
			}
		}

		/// <summary>
		/// Creates a row vector with 1 row, and the specified number of columns.
		/// </summary>
		public static MatrixMxN RowVector( int columns )
		{
			return new MatrixMxN( 1, columns );
		}
		
		/// <summary>
		/// Creates a column vector with 1 column, and the specified number of rows.
		/// </summary>
		public static MatrixMxN ColumnVector( int rows )
		{
			return new MatrixMxN( rows, 1 );
		}
		
		/// <summary>
		/// Adds rhs to lhs (element-wise).
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the matrices have different sizes.</exception>
		public static MatrixMxN Add( MatrixMxN lhs, MatrixMxN rhs )
		{
			if( lhs.Rows != rhs.Rows || lhs.Cols != rhs.Cols )
			{
				throw new InvalidOperationException( $"Can't add a Matrix{lhs.Rows}x{lhs.Cols} and a Matrix{rhs.Rows}x{rhs.Cols}." );
			}

			MatrixMxN result = new MatrixMxN( lhs.Rows, rhs.Cols );
			for( int i = 0; i < result._data.Length; i++ )
			{
				result._data[i] = lhs._data[i] + rhs._data[i];
			}
			return result;
		}
		
		/// <summary>
		/// Subtracts rhs from lhs (element-wise).
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the matrices have different sizes.</exception>
		public static MatrixMxN Subtract( MatrixMxN lhs, MatrixMxN rhs )
		{
			if( lhs.Rows != rhs.Rows || lhs.Cols != rhs.Cols )
			{
				throw new InvalidOperationException( $"Can't add a Matrix{lhs.Rows}x{lhs.Cols} and a Matrix{rhs.Rows}x{rhs.Cols}." );
			}

			MatrixMxN result = new MatrixMxN( lhs.Rows, rhs.Cols );
			for( int i = 0; i < result._data.Length; i++ )
			{
				result._data[i] = lhs._data[i] - rhs._data[i];
			}
			return result;
		}

		/// <summary>
		/// Multiplies every element of the matrix by a scalar.
		/// </summary>
		public static MatrixMxN Multiply( MatrixMxN m, float s )
		{
			MatrixMxN result = new MatrixMxN( m.Rows, m.Cols );
			for( int i = 0; i < result._data.Length; i++ )
			{
				result._data[i] = m._data[i] * s;
			}
			return result;
		}
		
		/// <summary>
		/// Divides every element of the matrix by a scalar.
		/// </summary>
		public static MatrixMxN Divide( MatrixMxN m, float s )
		{
			MatrixMxN result = new MatrixMxN( m.Rows, m.Cols );
			for( int i = 0; i < result._data.Length; i++ )
			{
				result._data[i] = m._data[i] / s;
			}
			return result;
		}

		/// <summary>
		/// Multiplies two matrices together.
		/// </summary>
		/// <remarks>
		/// E * m -> E performs row operations on m. <br/>
		/// m * E -> E performs column operations on m.
		/// </remarks>
		/// <exception cref="InvalidOperationException"></exception>
		public static MatrixMxN Multiply( MatrixMxN lhs, MatrixMxN rhs )
		{
			if( lhs.Rows != rhs.Cols )
			{
				throw new InvalidOperationException( $"Can't multiply a Matrix{lhs.Rows}x{lhs.Cols} with a Matrix{rhs.Rows}x{rhs.Cols}." );
			}

			MatrixMxN result = new MatrixMxN( lhs.Rows, rhs.Cols );
			for( int i = 0; i < lhs.Rows; i++ )
			{
				for( int j = 0; j < rhs.Cols; j++ )
				{
					float acc = 0;
					for( int k = 0; k < lhs.Cols; k++ )
					{
						acc += lhs[i, k] * rhs[k, j];
					}
					result[i, j] = acc;
				}
			}
			return result;
		}

		
		public static (MatrixMxN Q, MatrixMxN R) QRDecomposition( MatrixMxN mat )
		{
			// https://visualstudiomagazine.com/Articles/2024/01/03/matrix-inverse.aspx
			// QR decomposition, Householder algorithm.

			int rows = 3;
			int cols = 3;

			MatrixMxN Q = MatrixMxN.Identity(mat.Rows);
			MatrixMxN R = mat;

			for( int currentCol = 0; currentCol < cols - 1; currentCol++ )
			{
				MatrixMxN H = MatrixMxN.Identity(mat.Rows);
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

				Q = MatrixMxN.Multiply( Q, H );
				R = MatrixMxN.Multiply( H, R );
			}

			return (Q, R);
		}
	}
}