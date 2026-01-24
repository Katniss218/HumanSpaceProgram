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
        /// Private helper: creates a deep copy of a matrix.
        /// </summary>
        private static MatrixMxN Copy( MatrixMxN src )
        {
            MatrixMxN copy = new MatrixMxN( src.Rows, src.Cols );
            for( int i = 0; i < src.Rows; i++ )
                for( int j = 0; j < src.Cols; j++ )
                    copy[i, j] = src[i, j];
            return copy;
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
            if( lhs.Cols != rhs.Rows )
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

        public static MatrixMxN Solve( MatrixMxN A, MatrixMxN b )
        {
            if( A.Rows != A.Cols )
                throw new InvalidOperationException( "Solve requires a square coefficient matrix A." );
            if( b.Cols != 1 || b.Rows != A.Rows )
                throw new InvalidOperationException( "Right-hand side b must be a column vector with the same number of rows as A." );

            int n = A.Rows;
            MatrixMxN aug = new MatrixMxN( n, n + 1 );

            // build augmented matrix [A | b]
            for( int r = 0; r < n; r++ )
            {
                for( int c = 0; c < n; c++ )
                    aug[r, c] = A[r, c];
                aug[r, n] = b[r, 0];
            }

            const float EPS = 1e-9f;

            // forward elimination with partial pivoting
            for( int col = 0; col < n; col++ )
            {
                // find pivot row
                int pivot = col;
                float maxAbs = Mathf.Abs( aug[pivot, col] );
                for( int r = col + 1; r < n; r++ )
                {
                    float v = Mathf.Abs( aug[r, col] );
                    if( v > maxAbs )
                    {
                        maxAbs = v;
                        pivot = r;
                    }
                }

                if( maxAbs < EPS )
                    throw new InvalidOperationException( "Matrix is singular or nearly singular (no unique solution)." );

                // swap if needed
                if( pivot != col )
                {
                    for( int c = col; c < n + 1; c++ )
                    {
                        float tmp = aug[col, c];
                        aug[col, c] = aug[pivot, c];
                        aug[pivot, c] = tmp;
                    }
                }

                // normalize pivot row and eliminate below
                float pivotVal = aug[col, col];
                for( int c = col; c < n + 1; c++ )
                    aug[col, c] = aug[col, c] / pivotVal;

                for( int r = col + 1; r < n; r++ )
                {
                    float factor = aug[r, col];
                    if( factor == 0f ) continue;
                    for( int c = col; c < n + 1; c++ )
                        aug[r, c] -= factor * aug[col, c];
                }
            }

            // back substitution (since we normalized rows, diagonal is 1)
            MatrixMxN x = MatrixMxN.ColumnVector( n );
            for( int r = n - 1; r >= 0; r-- )
            {
                float val = aug[r, n]; // RHS
                for( int c = r + 1; c < n; c++ )
                    val -= aug[r, c] * x[c, 0];
                x[r, 0] = val; // diagonal is 1
            }

            return x;
        }

        /// <summary>
        /// Computes the inverse of a square matrix using Gauss-Jordan elimination with partial pivoting.
        /// Throws InvalidOperationException if matrix is not square or singular.
        /// </summary>
        public static MatrixMxN Inverse( MatrixMxN input )
        {
            if( input.Rows != input.Cols )
                throw new InvalidOperationException( "Inverse requires a square matrix." );

            int n = input.Rows;
            const float EPS = 1e-9f;

            // augmented [A | I]
            MatrixMxN aug = new MatrixMxN( n, 2 * n );
            for( int r = 0; r < n; r++ )
            {
                for( int c = 0; c < n; c++ )
                    aug[r, c] = input[r, c];
                for( int c = 0; c < n; c++ )
                    aug[r, n + c] = (r == c) ? 1f : 0f;
            }

            // Gauss-Jordan with partial pivoting
            for( int col = 0; col < n; col++ )
            {
                // pivot selection
                int pivot = col;
                float maxAbs = Mathf.Abs( aug[pivot, col] );
                for( int r = col + 1; r < n; r++ )
                {
                    float v = Mathf.Abs( aug[r, col] );
                    if( v > maxAbs )
                    {
                        maxAbs = v;
                        pivot = r;
                    }
                }

                if( maxAbs < EPS )
                    throw new InvalidOperationException( "Matrix is singular or nearly singular (cannot compute inverse)." );

                // swap
                if( pivot != col )
                {
                    for( int c = 0; c < 2 * n; c++ )
                    {
                        float tmp = aug[col, c];
                        aug[col, c] = aug[pivot, c];
                        aug[pivot, c] = tmp;
                    }
                }

                // normalize pivot row
                float pv = aug[col, col];
                for( int c = 0; c < 2 * n; c++ )
                    aug[col, c] /= pv;

                // eliminate all other rows
                for( int r = 0; r < n; r++ )
                {
                    if( r == col ) continue;
                    float factor = aug[r, col];
                    if( factor == 0f ) continue;
                    for( int c = 0; c < 2 * n; c++ )
                        aug[r, c] -= factor * aug[col, c];
                }
            }

            // extract right half as inverse
            MatrixMxN inv = new MatrixMxN( n, n );
            for( int r = 0; r < n; r++ )
                for( int c = 0; c < n; c++ )
                    inv[r, c] = aug[r, n + c];

            return inv;
        }

        /// <summary>
        /// Computes the determinant of a square matrix using LU-style elimination with partial pivoting.
        /// The algorithm performs row operations and counts row swaps to adjust the sign.
        /// </summary>
        public static float Determinant( MatrixMxN mat )
        {
            if( mat.Rows != mat.Cols )
                throw new InvalidOperationException( "Determinant requires a square matrix." );

            int n = mat.Rows;
            MatrixMxN a = Copy( mat );
            const float EPS = 1e-12f;
            float detSign = 1f;
            float detProd = 1f;

            for( int col = 0; col < n; col++ )
            {
                // find pivot
                int pivot = col;
                float maxAbs = Mathf.Abs( a[pivot, col] );
                for( int r = col + 1; r < n; r++ )
                {
                    float v = Mathf.Abs( a[r, col] );
                    if( v > maxAbs )
                    {
                        maxAbs = v;
                        pivot = r;
                    }
                }

                if( maxAbs < EPS )
                {
                    // singular -> determinant is zero
                    return 0f;
                }

                if( pivot != col )
                {
                    // swap rows
                    for( int c = col; c < n; c++ )
                    {
                        float tmp = a[col, c];
                        a[col, c] = a[pivot, c];
                        a[pivot, c] = tmp;
                    }
                    detSign = -detSign;
                }

                float pivotVal = a[col, col];
                detProd *= pivotVal;

                // eliminate below
                for( int r = col + 1; r < n; r++ )
                {
                    float factor = a[r, col] / pivotVal;
                    if( factor == 0f ) continue;
                    for( int c = col; c < n; c++ )
                        a[r, c] -= factor * a[col, c];
                }
            }

            return detSign * detProd;
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