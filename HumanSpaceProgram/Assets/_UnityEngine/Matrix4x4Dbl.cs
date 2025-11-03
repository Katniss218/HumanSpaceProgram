using System;

namespace UnityEngine
{
    [Obsolete( "untested" )]
	public struct Matrix4x4Dbl
	{
		public double m00, m01, m02, m03,
					  m10, m11, m12, m13,
					  m20, m21, m22, m23,
					  m30, m31, m32, m33;

        public Matrix4x4Dbl inverse => Inverse( this );

        public Matrix4x4Dbl transpose => Transpose( this );

        // lossyScale approximates scale by length of basis columns.
        // These are the column-vector lengths ignoring translation and perspective.
        public Vector3Dbl lossyScale
        {
            get
            {
                double sx = Math.Sqrt( m00 * m00 + m10 * m10 + m20 * m20 );
                double sy = Math.Sqrt( m01 * m01 + m11 * m11 + m21 * m21 );
                double sz = Math.Sqrt( m02 * m02 + m12 * m12 + m22 * m22 );

                // If determinant negative, we have a reflection. Flip one scale sign to indicate that.
                // Unity's lossyScale itself returns positive values, but when decomposing TRS Unity flips sign
                // on one axis when determinant < 0. If you only need magnitudes, remove the sign flip.
                double det = m00 * (m11 * m22 - m12 * m21)
                           - m01 * (m10 * m22 - m12 * m20)
                           + m02 * (m10 * m21 - m11 * m20);

                if( det < 0.0 )
                {
                    sx = -sx;
                }

                return new Vector3Dbl( sx, sy, sz );
            }
        }

        // Extract rotation (as quaternion) from the upper-left 3x3 portion.
        // This ignores scale; for correct rotation you should remove scale first if matrix has non-uniform scale/shear.
        public QuaternionDbl rotation
        {
            get
            {
                // Build a 3x3 rotation matrix R from upper-left of M (optionally normalize by scale)
                // We'll assume columns represent basis vectors (as usual). If scale!=1, this still returns the orientation component.
                double trace = m00 + m11 + m22;
                QuaternionDbl q = new QuaternionDbl();

                if( trace > 0.0 )
                {
                    double s = Math.Sqrt( trace + 1.0 ) * 2.0; // s = 4 * qw
                    q.w = 0.25 * s;
                    q.x = (m21 - m12) / s;
                    q.y = (m02 - m20) / s;
                    q.z = (m10 - m01) / s;
                }
                else if( (m00 > m11) && (m00 > m22) )
                {
                    double s = Math.Sqrt( 1.0 + m00 - m11 - m22 ) * 2.0; // s = 4 * qx
                    q.w = (m21 - m12) / s;
                    q.x = 0.25 * s;
                    q.y = (m01 + m10) / s;
                    q.z = (m02 + m20) / s;
                }
                else if( m11 > m22 )
                {
                    double s = Math.Sqrt( 1.0 + m11 - m00 - m22 ) * 2.0; // s = 4 * qy
                    q.w = (m02 - m20) / s;
                    q.x = (m01 + m10) / s;
                    q.y = 0.25 * s;
                    q.z = (m12 + m21) / s;
                }
                else
                {
                    double s = Math.Sqrt( 1.0 + m22 - m00 - m11 ) * 2.0; // s = 4 * qz
                    q.w = (m10 - m01) / s;
                    q.x = (m02 + m20) / s;
                    q.y = (m12 + m21) / s;
                    q.z = 0.25 * s;
                }

                // Normalize to be safe
                double mag = Math.Sqrt( q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w );
                if( mag > double.Epsilon )
                {
                    q.x /= mag; q.y /= mag; q.z /= mag; q.w /= mag;
                }
                return q;
            }
        }

        public Matrix4x4Dbl( double m00, double m01, double m02, double m03,
							 double m10, double m11, double m12, double m13,
							 double m20, double m21, double m22, double m23,
							 double m30, double m31, double m32, double m33 )
		{
			this.m00 = m00; this.m01 = m01; this.m02 = m02; this.m03 = m03;
			this.m10 = m10; this.m11 = m11; this.m12 = m12; this.m13 = m13;
			this.m20 = m20; this.m21 = m21; this.m22 = m22; this.m23 = m23;
			this.m30 = m30; this.m31 = m31; this.m32 = m32; this.m33 = m33;
		}
        public Vector4Dbl GetColumn( int index )
        {
            switch( index )
            {
                case 0: return new Vector4Dbl( m00, m10, m20, m30 );
                case 1: return new Vector4Dbl( m01, m11, m21, m31 );
                case 2: return new Vector4Dbl( m02, m12, m22, m32 );
                case 3: return new Vector4Dbl( m03, m13, m23, m33 );
                default: throw new IndexOutOfRangeException( "Invalid column index!" );
            }
        }

        public void SetColumn( int index, Vector4Dbl column )
        {
            switch( index )
            {
                case 0: m00 = column.x; m10 = column.y; m20 = column.z; m30 = column.w; break;
                case 1: m01 = column.x; m11 = column.y; m21 = column.z; m31 = column.w; break;
                case 2: m02 = column.x; m12 = column.y; m22 = column.z; m32 = column.w; break;
                case 3: m03 = column.x; m13 = column.y; m23 = column.z; m33 = column.w; break;
                default: throw new IndexOutOfRangeException( "Invalid column index!" );
            }
        }

        public Vector4Dbl GetRow( int index )
        {
            switch( index )
            {
                case 0: return new Vector4Dbl( m00, m01, m02, m03 );
                case 1: return new Vector4Dbl( m10, m11, m12, m13 );
                case 2: return new Vector4Dbl( m20, m21, m22, m23 );
                case 3: return new Vector4Dbl( m30, m31, m32, m33 );
                default: throw new IndexOutOfRangeException( "Invalid row index!" );
            }
        }

        public void SetRow( int index, Vector4Dbl row )
        {
            switch( index )
            {
                case 0: m00 = row.x; m01 = row.y; m02 = row.z; m03 = row.w; break;
                case 1: m10 = row.x; m11 = row.y; m12 = row.z; m13 = row.w; break;
                case 2: m20 = row.x; m21 = row.y; m22 = row.z; m23 = row.w; break;
                case 3: m30 = row.x; m31 = row.y; m32 = row.z; m33 = row.w; break;
                default: throw new IndexOutOfRangeException( "Invalid row index!" );
            }
        }

        public static Matrix4x4Dbl Transpose( Matrix4x4Dbl matrix )
		{
			return new Matrix4x4Dbl(
				matrix.m00, matrix.m10, matrix.m20, matrix.m30,
				matrix.m01, matrix.m11, matrix.m21, matrix.m31,
				matrix.m02, matrix.m12, matrix.m22, matrix.m32,
				matrix.m03, matrix.m13, matrix.m23, matrix.m33
			);
		}

		public static double Determinant( Matrix4x4Dbl matrix )
		{
			double _00 = matrix.m00, _01 = matrix.m01, _02 = matrix.m02, _03 = matrix.m03;
			double _10 = matrix.m10, _11 = matrix.m11, _12 = matrix.m12, _13 = matrix.m13;
			double _20 = matrix.m20, _21 = matrix.m21, _22 = matrix.m22, _23 = matrix.m23;
			double _30 = matrix.m30, _31 = matrix.m31, _32 = matrix.m32, _33 = matrix.m33;

			double kp_lo = (_22 * _33) - (_23 * _32);
			double jp_ln = (_21 * _33) - (_23 * _31);
			double jo_kn = (_21 * _32) - (_22 * _31);
			double ip_lm = (_20 * _33) - (_23 * _30);
			double io_km = (_20 * _32) - (_22 * _30);
			double in_jm = (_20 * _31) - (_21 * _30);

			double a11 = +(_11 * kp_lo - _12 * jp_ln + _13 * jo_kn);
			double a12 = -(_10 * kp_lo - _12 * ip_lm + _13 * io_km);
			double a13 = +(_10 * jp_ln - _11 * ip_lm + _13 * in_jm);
			double a14 = -(_10 * jo_kn - _11 * io_km + _12 * in_jm);

			double det = (_00 * a11) + (_01 * a12) + (_02 * a13) + (_03 * a14);
			return det;
		}

		public static Matrix4x4Dbl Inverse( Matrix4x4Dbl matrix )
		{
			double _00 = matrix.m00, _01 = matrix.m01, _02 = matrix.m02, _03 = matrix.m03;
			double _10 = matrix.m10, _11 = matrix.m11, _12 = matrix.m12, _13 = matrix.m13;
			double _20 = matrix.m20, _21 = matrix.m21, _22 = matrix.m22, _23 = matrix.m23;
			double _30 = matrix.m30, _31 = matrix.m31, _32 = matrix.m32, _33 = matrix.m33;

			double kp_lo = (_22 * _33) - (_23 * _32);
			double jp_ln = (_21 * _33) - (_23 * _31);
			double jo_kn = (_21 * _32) - (_22 * _31);
			double ip_lm = (_20 * _33) - (_23 * _30);
			double io_km = (_20 * _32) - (_22 * _30);
			double in_jm = (_20 * _31) - (_21 * _30);

			double a11 = +(_11 * kp_lo - _12 * jp_ln + _13 * jo_kn);
			double a12 = -(_10 * kp_lo - _12 * ip_lm + _13 * io_km);
			double a13 = +(_10 * jp_ln - _11 * ip_lm + _13 * in_jm);
			double a14 = -(_10 * jo_kn - _11 * io_km + _12 * in_jm);

			double det = (_00 * a11) + (_01 * a12) + (_02 * a13) + (_03 * a14);

			if( Math.Abs( det ) < double.Epsilon )
			{
				throw new Exception( "Matrix is not invertible" );
			}

			double invDet = 1.0f / det;

			double gp_ho = _12 * _33 - _13 * _32;
			double fp_hn = _11 * _33 - _13 * _31;
			double fo_gn = _11 * _32 - _12 * _31;
			double ep_hm = _10 * _33 - _13 * _30;
			double eo_gm = _10 * _32 - _12 * _30;
			double en_fm = _10 * _31 - _11 * _30;

			double gl_hk = _12 * _23 - _13 * _22;
			double fl_hj = _11 * _23 - _13 * _21;
			double fk_gj = _11 * _22 - _12 * _21;
			double el_hi = _10 * _23 - _13 * _20;
			double ek_gi = _10 * _22 - _12 * _20;
			double ej_fi = _10 * _21 - _11 * _20;

			return new Matrix4x4Dbl(
				a11 * invDet, -(_01 * kp_lo - _02 * jp_ln + _03 * jo_kn) * invDet, +(_01 * gp_ho - _02 * fp_hn + _03 * fo_gn) * invDet, -(_01 * gl_hk - _02 * fl_hj + _03 * fk_gj) * invDet,
				a12 * invDet, +(_00 * kp_lo - _02 * ip_lm + _03 * io_km) * invDet, -(_00 * gp_ho - _02 * ep_hm + _03 * eo_gm) * invDet, +(_00 * gl_hk - _02 * el_hi + _03 * ek_gi) * invDet,
				a13 * invDet, -(_00 * jp_ln - _01 * ip_lm + _03 * in_jm) * invDet, +(_00 * fp_hn - _01 * ep_hm + _03 * en_fm) * invDet, -(_00 * fl_hj - _01 * el_hi + _03 * ej_fi) * invDet,
				a14 * invDet, +(_00 * jo_kn - _01 * io_km + _02 * in_jm) * invDet, -(_00 * fo_gn - _01 * eo_gm + _02 * en_fm) * invDet, +(_00 * fk_gj - _01 * ek_gi + _02 * ej_fi) * invDet
			);
		}

        public static Matrix4x4Dbl Rotate( QuaternionDbl rotation )
		{
			double xSq = rotation.x * rotation.x;
			double ySq = rotation.y * rotation.y;
			double zSq = rotation.z * rotation.z;

			double xy = rotation.x * rotation.y;
			double xz = rotation.x * rotation.z;
			double yz = rotation.y * rotation.z;
			double wx = rotation.w * rotation.x;
			double wy = rotation.w * rotation.y;
			double wz = rotation.w * rotation.z;

			return new Matrix4x4Dbl(
				1 - 2 * (ySq + zSq), 2 * (xy - wz), 2 * (xz + wy), 0,
				2 * (xy + wz), 1 - 2 * (xSq + zSq), 2 * (yz - wx), 0,
				2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (xSq + ySq), 0,
				0, 0, 0, 1
			);
		}
        public static Matrix4x4Dbl Scale( Vector3Dbl vector )
		{

			return new Matrix4x4Dbl(
				vector.x, 0, 0, 0,
				0, vector.y, 0, 0,
				0, 0, vector.z, 0,
				0, 0, 0, 1
			);
		}
        public static Matrix4x4Dbl Translate( Vector3Dbl vector )
		{
			return new Matrix4x4Dbl(
				1, 0, 0, vector.x,
				0, 1, 0, vector.y,
				0, 0, 1, vector.z,
				0, 0, 0, 1
			);
        }
        
        // TRS: Translate * Rotate * Scale (Unity convention)
        public static Matrix4x4Dbl TRS( Vector3Dbl pos, QuaternionDbl q, Vector3Dbl s )
        {
            // M = T * R * S
            Matrix4x4Dbl t = Translate( pos );
            Matrix4x4Dbl r = Rotate( q );
            Matrix4x4Dbl sc = Scale( s );
            return t * r * sc;
        }



        // MultiplyPoint3x4: affine transform (no perspective divide)
        public Vector3Dbl MultiplyPoint3x4( Vector3Dbl v )
        {
            return new Vector3Dbl(
                this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03,
                this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13,
                this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23
            );
        }

        // MultiplyPoint: includes homogeneous w (perspective), then divides by w
        // We compute (x',y',z',w') = M * (x,y,z,1). If w' != 1, we divide xyz by w'.
        public Vector3Dbl MultiplyPoint( Vector3Dbl v )
        {
            double x = m00 * v.x + m01 * v.y + m02 * v.z + m03;
            double y = m10 * v.x + m11 * v.y + m12 * v.z + m13;
            double z = m20 * v.x + m21 * v.y + m22 * v.z + m23;
            double w = m30 * v.x + m31 * v.y + m32 * v.z + m33;

            if( Math.Abs( w - 1.0 ) > 1e-12 && Math.Abs( w ) > double.Epsilon )
            {
                x /= w; y /= w; z /= w;
            }
            return new Vector3Dbl( x, y, z );
        }

        // Transforms a direction by this matrix (ignores translation)
        public Vector3Dbl MultiplyVector( Vector3Dbl v )
        {
            return new Vector3Dbl(
                this.m00 * v.x + this.m01 * v.y + this.m02 * v.z,
                this.m10 * v.x + this.m11 * v.y + this.m12 * v.z,
                this.m20 * v.x + this.m21 * v.y + this.m22 * v.z
            );
        }

        public override bool Equals( object obj )
        {
            if( !(obj is Matrix4x4Dbl) ) 
                return false;

            Matrix4x4Dbl o = (Matrix4x4Dbl)obj;
            return m00 == o.m00 && m01 == o.m01 && m02 == o.m02 && m03 == o.m03 &&
                   m10 == o.m10 && m11 == o.m11 && m12 == o.m12 && m13 == o.m13 &&
                   m20 == o.m20 && m21 == o.m21 && m22 == o.m22 && m23 == o.m23 &&
                   m30 == o.m30 && m31 == o.m31 && m32 == o.m32 && m33 == o.m33;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + m00.GetHashCode();
                hash = hash * 23 + m01.GetHashCode();
                hash = hash * 23 + m02.GetHashCode();
                hash = hash * 23 + m03.GetHashCode();
                hash = hash * 23 + m10.GetHashCode();
                hash = hash * 23 + m11.GetHashCode();
                hash = hash * 23 + m12.GetHashCode();
                hash = hash * 23 + m13.GetHashCode();
                hash = hash * 23 + m20.GetHashCode();
                hash = hash * 23 + m21.GetHashCode();
                hash = hash * 23 + m22.GetHashCode();
                hash = hash * 23 + m23.GetHashCode();
                hash = hash * 23 + m30.GetHashCode();
                hash = hash * 23 + m31.GetHashCode();
                hash = hash * 23 + m32.GetHashCode();
                hash = hash * 23 + m33.GetHashCode();
                return hash;
            }
        }

        public static Vector4Dbl operator *( Matrix4x4Dbl m, Vector4Dbl v )
        {
            return new Vector4Dbl(
                m.m00 * v.x + m.m01 * v.y + m.m02 * v.z + m.m03 * v.w,
                m.m10 * v.x + m.m11 * v.y + m.m12 * v.z + m.m13 * v.w,
                m.m20 * v.x + m.m21 * v.y + m.m22 * v.z + m.m23 * v.w,
                m.m30 * v.x + m.m31 * v.y + m.m32 * v.z + m.m33 * v.w
            );
        }
        
        public static Vector3Dbl operator *( Matrix4x4Dbl m, Vector3Dbl v )
        {
            return new Vector3Dbl(
                m.m00 * v.x + m.m01 * v.y + m.m02 * v.z,
                m.m10 * v.x + m.m11 * v.y + m.m12 * v.z,
                m.m20 * v.x + m.m21 * v.y + m.m22 * v.z
            );
        }

        public static Matrix4x4Dbl operator *( Matrix4x4Dbl lhs, Matrix4x4Dbl rhs )
        {
            Matrix4x4Dbl r = new Matrix4x4Dbl();
            r.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
            r.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
            r.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
            r.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

            r.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
            r.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
            r.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
            r.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

            r.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
            r.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
            r.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
            r.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

            r.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
            r.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
            r.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
            r.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;
            return r;
        }

        public static Matrix4x4Dbl operator -( Matrix4x4Dbl lhs, Matrix4x4Dbl rhs )
        {
            Matrix4x4Dbl result = new Matrix4x4Dbl();

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
        public static bool operator ==( Matrix4x4Dbl a, Matrix4x4Dbl b ) => a.Equals( b );
        public static bool operator !=( Matrix4x4Dbl a, Matrix4x4Dbl b ) => !a.Equals( b );

        public override string ToString()
        {
            return $"Matrix4x4Dbl(\n  {m00:F6} {m01:F6} {m02:F6} {m03:F6}\n  {m10:F6} {m11:F6} {m12:F6} {m13:F6}\n  {m20:F6} {m21:F6} {m22:F6} {m23:F6}\n  {m30:F6} {m31:F6} {m32:F6} {m33:F6}\n)";
        }
    }
}