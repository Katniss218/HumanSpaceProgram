using System;

namespace UnityPlus
{
    /// <summary>
    /// A curve that that maps input of type <typeparamref name="T"/> onto output directly, without any interpolation.
    /// </summary>
    public class LinearMappingCurve<T> : IMappingCurve<T>
    {
        public IMappingCurveInterpolator<T> Interpolator { get; }

        public LinearMappingCurve( IMappingCurveInterpolator<T> interpolator )
        {
            this.Interpolator = interpolator ?? throw new ArgumentNullException( nameof( interpolator ) );
        }

        public T Evaluate( T time )
        {
            return time;
        }

        public T Evaluate( T time, IMappingCurveInterpolator<T> interpolator )
        {
            return time;
        }
    }
}