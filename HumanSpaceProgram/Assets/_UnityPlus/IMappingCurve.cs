namespace UnityPlus
{
    public interface IMappingCurve<T>
    {
        public IMappingCurveInterpolator<T> Interpolator { get; }
        public T Evaluate( T time );
        public T Evaluate( T time, IMappingCurveInterpolator<T> interpolator );
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
