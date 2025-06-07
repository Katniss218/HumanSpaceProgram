using System;
using System.Collections.Generic;

namespace UnityPlus
{
    /// <summary>
    /// Represents a generic curve composed of keyframes that maps input of type <typeparamref name="T"/> onto output of the same type.
    /// </summary>
    public class MappingCurve<T> : IMappingCurve<T>
    {
        /// <summary>
        /// A keyframe in the curve.
        /// </summary>
        public struct Keyframe
        {
            public T Time { get; set; }
            public T Value { get; set; }
            public T InTangent { get; set; }
            public T OutTangent { get; set; }

            public Keyframe( T time, T value, T inTangent, T outTangent )
            {
                Time = time;
                Value = value;
                InTangent = inTangent;
                OutTangent = outTangent;
            }
        }

        public IMappingCurveInterpolator<T> Interpolator { get; }

        private List<Keyframe> _keyframes = new();
        public List<Keyframe> Keyframes
        {
            get => _keyframes;
            set
            {
                if( value == null )
                    throw new ArgumentNullException( nameof( value ) );

                _keyframes = value;
                _keyframes.Sort( ( l, r ) => Interpolator.Compare( l.Time, r.Time ) );
            }
        }

        public MappingCurve( IMappingCurveInterpolator<T> interpolator )
        {
            this.Interpolator = interpolator ?? throw new ArgumentNullException( nameof( interpolator ) );
        }

        public void AddKeyframe( Keyframe keyframe )
        {
            _keyframes.Add( keyframe );
            _keyframes.Sort( ( l, r ) => this.Interpolator.Compare( l.Time, r.Time ) );
        }

        public T Evaluate( T time )
        {
            return Evaluate( time, this.Interpolator );
        }

        public T Evaluate( T time, IMappingCurveInterpolator<T> interpolator )
        {
            if( _keyframes == null || _keyframes.Count == 0 )
                return time;

            if( interpolator.Compare( time, _keyframes[0].Time ) <= 0 )
                return _keyframes[0].Value;
            if( interpolator.Compare( time, _keyframes[^1].Time ) >= 0 )
                return _keyframes[^1].Value;

            int index = _keyframes.FindIndex( kf => interpolator.Compare( kf.Time, time ) > 0 ) - 1;
            var kf0 = _keyframes[index];
            var kf1 = _keyframes[index + 1];

            T t = interpolator.Divide( interpolator.Subtract( time, kf0.Time ), interpolator.Subtract( kf1.Time, kf0.Time ) );
            return interpolator.Interpolate( kf0.Value, kf1.Value, kf0.OutTangent, kf1.InTangent, t );
        }

        public void Clear()
        {
            _keyframes.Clear();
        }
    }
}