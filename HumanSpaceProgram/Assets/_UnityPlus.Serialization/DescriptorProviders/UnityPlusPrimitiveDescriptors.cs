using UnityEngine;
using UnityPlus.Serialization.Descriptors;

namespace UnityPlus.Serialization.DescriptorProviders
{
    public static class UnityPlusPrimitiveDescriptors
    {
        // --- VECTORS ---

        [MapsInheritingFrom( typeof( Vector3Dbl ) )]
        public static IDescriptor Vector3DblDescriptor() => new PrimitiveConfigurableDescriptor<Vector3Dbl>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 3) ? new Vector3Dbl( (double)arr[0], (double)arr[1], (double)arr[2] ) : default
        );

        // --- QUATERNION ---

        [MapsInheritingFrom( typeof( QuaternionDbl ) )]
        public static IDescriptor QuaternionDblDescriptor() => new PrimitiveConfigurableDescriptor<QuaternionDbl>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z, (SerializedPrimitive)target.w },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 4) ? new QuaternionDbl( (double)arr[0], (double)arr[1], (double)arr[2], (double)arr[3] ) : default
        );

        [MapsInheritingFrom( typeof( FloatInterpolator ) )]
        public static IDescriptor FloatInterpolatorMapping()
        {
            return new MemberwiseDescriptor<FloatInterpolator>();
        }

        [MapsInheritingFrom( typeof( MappingCurve<>.Keyframe ) )]
        public static IDescriptor KeyframeMapping<T>()
        {
            return new MemberwiseDescriptor<MappingCurve<T>.Keyframe>()
                .WithMember( "t", o => o.Time )
                .WithMember( "v", o => o.Value )
                .WithMember( "v_in", o => o.InTangent )
                .WithMember( "v_out", o => o.OutTangent );
        }

        [MapsInheritingFrom( typeof( MappingCurve<> ) )]
        public static IDescriptor AnimationCurveMapping<T>()
        {
            return new MemberwiseDescriptor<MappingCurve<T>>()
                .WithReadonlyMember( "interpolator", o => o.Interpolator )
                .WithFactory<IMappingCurveInterpolator<T>>( ( interpolator ) => new MappingCurve<T>( interpolator ), "interpolator" )
                .WithMember( "keys", o => o.Keyframes );
        }

        [MapsInheritingFrom( typeof( LinearMappingCurve<> ) )]
        public static IDescriptor LinearMappingCurveMapping<T>()
        {
            return new MemberwiseDescriptor<LinearMappingCurve<T>>()
                .WithReadonlyMember( "interpolator", o => o.Interpolator )
                .WithFactory<IMappingCurveInterpolator<T>>( ( interpolator ) => new LinearMappingCurve<T>( interpolator ), "interpolator" );
        }
    }
}