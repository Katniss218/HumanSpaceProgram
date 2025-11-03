using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    public static class Matrix3x3_Ex
    {
        /// <summary>
        /// Solve A * x = b for x using Cramer's rule. Returns false if matrix is singular (|det| <= epsilon).
        /// Uses determinants of 3x3 and 3x3-with-column-replaced calculations, so it's branchless and allocation-free.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TrySolve( this Matrix3x3 m, Vector3 b, out Vector3 x, float epsilon = 1e-6f )
        {
            // Cramer's rule: x_i = det(A_i) / det(A)
            // where A_i is A with column i replaced by b.
            float detA = m.Determinant();
            if( !float.IsFinite( detA ) || Mathf.Abs( detA ) <= epsilon )
            {
                x = Vector3.zero;
                return false;
            }

            // detA_x: replace column 0 with b
            float detAx =
                (b.x * (m.m11 * m.m22 - m.m12 * m.m21))
              - (m.m01 * (b.y * m.m22 - m.m12 * b.z))
              + (m.m02 * (b.y * m.m21 - m.m11 * b.z));

            // detA_y: replace column 1 with b
            float detAy =
                (m.m00 * (b.y * m.m22 - m.m12 * b.z))
              - (b.x * (m.m10 * m.m22 - m.m12 * m.m20))
              + (m.m02 * (m.m10 * b.z - b.y * m.m20));

            // detA_z: replace column 2 with b
            float detAz =
                (m.m00 * (m.m11 * b.z - b.y * m.m21))
              - (m.m01 * (m.m10 * b.z - b.y * m.m20))
              + (b.x * (m.m10 * m.m21 - m.m11 * m.m20));

            float invDet = 1.0f / detA;
            x = new Vector3( detAx * invDet, detAy * invDet, detAz * invDet );
            return true;
        }

        /// <summary>
        /// Solve A * x = b and return x. Throws InvalidOperationException if matrix singular.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 Solve( this Matrix3x3 m, Vector3 b, float epsilon = 1e-6f )
        {
            if( TrySolve( m, b, out Vector3 x, epsilon ) )
                return x;
            throw new InvalidOperationException( "Matrix is singular or nearly singular; cannot solve." );
        }

        /// <summary>
		/// Solve multiple RHS: A * X = B where B has N columns (each column is a Vector3).
		/// This overload uses Spans for allocation-free operation. Returns false if singular.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TrySolve( this Matrix3x3 m, ReadOnlySpan<Vector3> b, Span<Vector3> x, float epsilon = 1e-6f )
        {
            // Expect b.Length == x.Length, each element is a Vector3 representing a RHS column.
            if( b.Length != x.Length )
                throw new ArgumentException( "b and x must have equal length." );

            float detA = m.Determinant();
            if( !float.IsFinite( detA ) || Mathf.Abs( detA ) <= epsilon )
            {
                // fill output with zero for clarity
                for( int i = 0; i < x.Length; ++i ) x[i] = Vector3.zero;
                return false;
            }

            // Precompute sub-determinants used repeatedly to reduce repeated arithmetic.
            float d11 = m.m11 * m.m22 - m.m12 * m.m21;
            float d12 = m.m10 * m.m22 - m.m12 * m.m20;
            float d13 = m.m10 * m.m21 - m.m11 * m.m20;

            for( int i = 0; i < b.Length; ++i )
            {
                Vector3 bi = b[i];

                // Cramer's rule: detAi = det(A with column i replaced by b)
                // Here we compute determinants for each column replacement:
                float detAx =
                    (bi.x * d11)
                 - (m.m01 * (bi.y * m.m22 - m.m12 * bi.z))
                 + (m.m02 * (bi.y * m.m21 - m.m11 * bi.z));

                float detAy =
                    (m.m00 * (bi.y * m.m22 - m.m12 * bi.z))
                 - (bi.x * d12)
                 + (m.m02 * (m.m10 * bi.z - bi.y * m.m20));

                float detAz =
                    (m.m00 * (m.m11 * bi.z - bi.y * m.m21))
                 - (m.m01 * (m.m10 * bi.z - bi.y * m.m20))
                 + (bi.x * d13);

                float invDet = 1.0f / detA;
                x[i] = new Vector3( detAx * invDet, detAy * invDet, detAz * invDet );
            }

            return true;
        }

        /// <summary>
        /// Solve multiple RHS: A * X = B where B is a Vector3[] of length N and X is returned as new Vector3[].
        /// Returns false if singular; out param x will be zero-filled in that case.
        /// </summary>
        public static bool TrySolve( this Matrix3x3 m, Vector3[] b, out Vector3[] x, float epsilon = 1e-6f )
        {
            if( b == null ) throw new ArgumentNullException( nameof( b ) );
            x = new Vector3[b.Length];
            return TrySolve( m, b.AsSpan(), x.AsSpan(), epsilon );
        }

        /// <summary>
        /// Convenience wrapper that throws on failure for Vector3[].
        /// </summary>
        public static Vector3[] Solve( this Matrix3x3 m, Vector3[] b, float epsilon = 1e-6f )
        {
            if( !TrySolve( m, b, out Vector3[] x, epsilon ) )
                throw new InvalidOperationException( "Matrix is singular or nearly singular; cannot solve." );
            return x;
        }

        public static Matrix3x3 GetCofactorMatrix( this Matrix3x3 m )
        {
            // Each cofactor C_ij = (-1)^(i+j) * det(Minor_ij)
            float c00 = (m.m11 * m.m22 - m.m12 * m.m21);
            float c01 = -(m.m10 * m.m22 - m.m12 * m.m20);
            float c02 = (m.m10 * m.m21 - m.m11 * m.m20);

            float c10 = -(m.m01 * m.m22 - m.m02 * m.m21);
            float c11 = (m.m00 * m.m22 - m.m02 * m.m20);
            float c12 = -(m.m00 * m.m21 - m.m01 * m.m20);

            float c20 = (m.m01 * m.m12 - m.m02 * m.m11);
            float c21 = -(m.m00 * m.m12 - m.m02 * m.m10);
            float c22 = (m.m00 * m.m11 - m.m01 * m.m10);

            return new Matrix3x3(
                c00, c01, c02,
                c10, c11, c12,
                c20, c21, c22
            );
        }

        /// <summary>
        /// Adjugate (adjoint) = transpose of cofactor matrix.
        /// Inverse(A) = adj(A) / det(A)
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Matrix3x3 Adjugate( this Matrix3x3 m )
        {
            // Adjugate is transpose of cofactor matrix; compute directly for slightly fewer temporaries.
            return new Matrix3x3(
                (m.m11 * m.m22 - m.m12 * m.m21), (m.m10 * m.m22 - m.m12 * m.m20) * -1f, (m.m10 * m.m21 - m.m11 * m.m20),
                (m.m01 * m.m22 - m.m02 * m.m21) * -1f, (m.m00 * m.m22 - m.m02 * m.m20), (m.m00 * m.m21 - m.m01 * m.m20) * -1f,
                (m.m01 * m.m12 - m.m02 * m.m11), (m.m00 * m.m12 - m.m02 * m.m10) * -1f, (m.m00 * m.m11 - m.m01 * m.m10)
            );
        }

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

                // Eigenvectors can switch sign every iteration, because the Q converges on having negative values.
                // But when normalized, and ignoring the sign, they are correct.
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

        public static bool IsDiagonal( this Matrix3x3 mat )
        {
            const float epsilon = 1e-6f;

            return Mathf.Abs( mat.m01 ) < epsilon &&
                   Mathf.Abs( mat.m02 ) < epsilon &&
                   Mathf.Abs( mat.m10 ) < epsilon &&
                   Mathf.Abs( mat.m12 ) < epsilon &&
                   Mathf.Abs( mat.m20 ) < epsilon &&
                   Mathf.Abs( mat.m21 ) < epsilon;
        }

        static Quaternion PhysxIndexedRotation( int axis, float s, float c )
        {
            float[] v = { 0, 0, 0 };
            v[axis] = s;
            return new Quaternion( v[0], v[1], v[2], c );
        }

        public static Vector3 PhysxDiagonalize( this Matrix3x3 m, out Quaternion massFrame )
        {
            // From nvidia physx https://github.com/NVIDIAGameWorks/PhysX/blob/4.1/physx/source/foundation/src/PsMathUtils.cpp
            // jacobi rotation using quaternions (from an idea of Stan Melax, with fix for precision issues)

            const int MAX_ITERS = 24;

            Quaternion q = Quaternion.identity;

            Matrix3x3 d = Matrix3x3.zero;

            for( int i = 0; i < MAX_ITERS; i++ )
            {
                Matrix3x3 axes = Matrix3x3.Rotate( q );
                d = axes.transpose * m * axes;

                float d0 = Mathf.Abs( d[1, 2] ), d1 = Mathf.Abs( d[0, 2] ), d2 = Mathf.Abs( d[0, 1] );
                int a = (d0 > d1 && d0 > d2) ? 0 : d1 > d2 ? 1 : 2; // rotation axis index, from largest off-diagonal element

                int a1 = (a + 1) % 3;
                int a2 = (a1 + 1) % 3;

                if( d[a1, a2] == 0.0f || Mathf.Abs( d[a1, a1] - d[a2, a2] ) > 2e6f * Mathf.Abs( 2.0f * d[a1, a2] ) )
                    break;

                float w = (d[a1, a1] - d[a2, a2]) / (2.0f * d[a1, a2]); // cot(2 * phi), where phi is the rotation angle
                float absw = Mathf.Abs( w );

                Quaternion r;
                if( absw > 1000 )
                    r = PhysxIndexedRotation( a, 1 / (4 * w), 1.0f ); // h will be very close to 1, so use small angle approx instead
                else
                {
                    float t = 1 / (absw + Mathf.Sqrt( w * w + 1 )); // absolute value of tan phi
                    float h = 1 / Mathf.Sqrt( t * t + 1 );          // absolute value of cos phi

                    //PX_ASSERT( h != 1 ); // |w|<1000 guarantees this with typical IEEE754 machine eps (approx 6e-8)
                    r = PhysxIndexedRotation( a, Mathf.Sqrt( (1 - h) / 2 ) * Mathf.Sign( w ), Mathf.Sqrt( (1 + h) / 2 ) );
                }

                q = (q * r).normalized;
            }

            massFrame = q;
            return new Vector3( d.m00, d.m11, d.m22 );
        }
    }
}