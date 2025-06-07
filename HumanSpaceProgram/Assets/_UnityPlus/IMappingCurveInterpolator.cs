using System.Collections.Generic;

namespace UnityPlus
{
    /// <summary>
    /// Represents a generic interpolator for a <see cref="MappingCurve{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type that is being interpolated.</typeparam>
    public interface IMappingCurveInterpolator<T> : IComparer<T>, IEqualityComparer<T>
    {
        /// <summary>
        /// Adds the value of <paramref name="b"/> to <paramref name="a"/>.
        /// </summary>
        T Add( T a, T b );

        /// <summary>
        /// Subtracts the value of <paramref name="b"/> from <paramref name="a"/>.
        /// </summary>
        T Subtract( T a, T b );

        /// <summary>
        /// Multiplies <paramref name="a"/> by <paramref name="b"/>.
        /// </summary>
        T Multiply( T a, T b );

        /// <summary>
        /// Divides <paramref name="a"/> by <paramref name="b"/>.
        /// </summary>
        T Divide( T a, T b );

        /// <summary>
        /// Interpolates between two values.
        /// </summary>
        /// <param name="v0">The first value.</param>
        /// <param name="v1">The second value</param>
        /// <param name="outTangent0">The derivative of the first value at time 0.</param>
        /// <param name="inTangent1">The derivative of the second value at time 1.</param>
        /// <param name="t">The 'time'.</param>
        public T Interpolate( T v0, T v1, T outTangent0, T inTangent1, T t );
    }
}