using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.Networking.UnityWebRequest;

namespace UnityEngine
{
	[Obsolete( "untested" )]
	public struct Matrix4x4Dbl
	{
		public double m00, m01, m02, m03,
					  m10, m11, m12, m13,
					  m20, m21, m22, m23,
					  m30, m31, m32, m33;

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

		public Matrix4x4Dbl transpose => Transpose( this );

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

		public Matrix4x4Dbl inverse => Inverse( this );

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

		Matrix4x4Dbl Rotate( QuaternionDbl rotation )
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
		Matrix4x4Dbl Scale( Vector3Dbl vector )
		{

			return new Matrix4x4Dbl(
				vector.x, 0, 0, 0,
				0, vector.y, 0, 0,
				0, 0, vector.z, 0,
				0, 0, 0, 1
			);
		}
		Matrix4x4Dbl Translate( Vector3Dbl vector )
		{
			return new Matrix4x4Dbl(
				1, 0, 0, vector.x,
				0, 1, 0, vector.y,
				0, 0, 1, vector.z,
				0, 0, 0, 1
			);
		}

		public Vector3Dbl MultiplyPoint3x4( Vector3Dbl v )
		{
			return new Vector3Dbl(
				this.m00 * v.x + this.m01 * v.y + this.m02 * v.z + this.m03,
				this.m10 * v.x + this.m11 * v.y + this.m12 * v.z + this.m13,
				this.m20 * v.x + this.m21 * v.y + this.m22 * v.z + this.m23
			);
		}

		// Transforms a direction by this matrix.
		public Vector3Dbl MultiplyVector( Vector3Dbl v )
		{
			// same as unity's matrix4x4
			return new Vector3Dbl(
				this.m00 * v.x + this.m01 * v.y + this.m02 * v.z,
				this.m10 * v.x + this.m11 * v.y + this.m12 * v.z,
				this.m20 * v.x + this.m21 * v.y + this.m22 * v.z
			);
		}
	}
}