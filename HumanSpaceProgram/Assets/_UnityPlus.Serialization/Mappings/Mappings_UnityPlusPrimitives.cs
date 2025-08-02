using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityPlusPrimitives
    {
        [MapsInheritingFrom( typeof( Vector3Dbl ) )]
        public static SerializationMapping Vector3DblMapping()
        {
            return new PrimitiveSerializationMapping<Vector3Dbl>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnLoad = ( data, l ) => new Vector3Dbl( (double)data[0], (double)data[1], (double)data[2] )
            };
        }

        [MapsInheritingFrom( typeof( QuaternionDbl ) )]
        public static SerializationMapping QuaternionDblMapping()
        {
            return new PrimitiveSerializationMapping<QuaternionDbl>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnLoad = ( data, l ) => new QuaternionDbl( (double)data[0], (double)data[1], (double)data[2], (double)data[3] )
            };
        }

        [MapsInheritingFrom( typeof( FloatInterpolator ) )]
        public static SerializationMapping FloatInterpolatorMapping()
        {
            return new MemberwiseSerializationMapping<FloatInterpolator>();
        }

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
}