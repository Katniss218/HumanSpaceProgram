using UnityEngine;

namespace UnityPlus
{
    /// <summary>
    /// Interpolates between two float values using Hermite interpolation.
    /// </summary>
    public class FloatInterpolator : IMappingCurveInterpolator<float>
    {
        public float Add( float a, float b ) => a + b;
        public float Subtract( float a, float b ) => a - b;
        public float Multiply( float a, float b ) => a * b;
        public float Divide( float a, float b ) => a / b;

        /// <summary>
        /// Hermite interpolation for floats.
        /// </summary>
        public float Interpolate( float v0, float v1, float outTangent0, float inTangent1, float t )
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;
            return h00 * v0 + h10 * outTangent0 + h01 * v1 + h11 * inTangent1;
        }

        public int Compare( float x, float y ) => x.CompareTo( y );

        public bool Equals( float x, float y ) => x.Equals( y );

        public int GetHashCode( float obj ) => obj.GetHashCode();
    }

    /*
    /// <summary>
    /// Common interpolators for specific types.
    /// </summary>
    public static class Curve_T_Interpolators
    {
        /// <summary>
        /// Component-wise Hermite interpolation for Vector3.
        /// </summary>
        public static Vector3 Vector3Hermite( Vector3 v0, Vector3 v1, Vector3 outTangent0, Vector3 inTangent1, float t )
        {
            return new Vector3(
                FloatHermite( v0.x, v1.x, outTangent0.x, inTangent1.x, t ),
                FloatHermite( v0.y, v1.y, outTangent0.y, inTangent1.y, t ),
                FloatHermite( v0.z, v1.z, outTangent0.z, inTangent1.z, t )
            );
        }

        /// <summary>
        /// SQUAD (spherical and quadrangle) interpolation for Quaternion using out/in tangents.
        /// </summary>
        public static Quaternion QuaternionSquad( Quaternion q0, Quaternion q1, Quaternion outTangent0, Quaternion inTangent1, float t )
        {
            // First slerp between q0->outTangent0 and inTangent1->q1
            Quaternion s1 = Quaternion.Slerp( q0, outTangent0, t );
            Quaternion s2 = Quaternion.Slerp( inTangent1, q1, t );
            // Then slerp between the results
            return Quaternion.Slerp( s1, s2, 2f * t * (1f - t) );
        }
    }*/
}