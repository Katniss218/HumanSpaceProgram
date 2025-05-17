using System;
using System.Collections.Generic;
using UnityPlus.Serialization;

namespace HSP.Audio
{
    internal static class MappingCurve_T__Mapping
    {
        [MapsInheritingFrom( typeof( MappingCurve<>.Keyframe ) )]
        public static SerializationMapping KeyframeMapping<T>()
        {
            return new MemberwiseSerializationMapping<MappingCurve<T>.Keyframe>()
                .WithMember( "t", o => o.Time )
                .WithMember( "v", o => o.Value )
                .WithMember( "v_in", o => o.InTangent )
                .WithMember( "v_out", o => o.OutTangent );
        }

        [MapsInheritingFrom( typeof( MappingCurve<> ) )]
        public static SerializationMapping AnimationCurveMapping<T>()
        {
            return new MemberwiseSerializationMapping<MappingCurve<T>>()
                .WithReadonlyMember( "interpolator", o => o.Interpolator )
                .WithFactory<IMappingCurveInterpolator<T>>( ( interpolator ) => new MappingCurve<T>( interpolator ) )
                .WithMember( "keys", o => o.Keyframes );
        }

        [MapsInheritingFrom( typeof( LinearMappingCurve<> ) )]
        public static SerializationMapping LinearMappingCurveMapping<T>()
        {
            return new MemberwiseSerializationMapping<LinearMappingCurve<T>>()
                .WithReadonlyMember( "interpolator", o => o.Interpolator )
                .WithFactory<IMappingCurveInterpolator<T>>( ( interpolator ) => new LinearMappingCurve<T>( interpolator ) );
        }
    }

    public interface IMappingCurve<T>
    {
        public IMappingCurveInterpolator<T> Interpolator { get; }
        public T Evaluate( T time );
        public T Evaluate( T time, IMappingCurveInterpolator<T> interpolator );
    }

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

    public interface IMappingCurveInterpolator<T> : IComparer<T>, IEqualityComparer<T>
    {
        T Add( T a, T b );

        T Subtract( T a, T b );

        T Multiply( T a, T b );

        T Divide( T a, T b );

        public T Interpolate( T v0, T v1, T outTangent0, T inTangent1, T t );
    }

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


        [MapsInheritingFrom( typeof( FloatInterpolator ) )]
        public static SerializationMapping FloatInterpolatorMapping()
        {
            return new MemberwiseSerializationMapping<FloatInterpolator>();
        }
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