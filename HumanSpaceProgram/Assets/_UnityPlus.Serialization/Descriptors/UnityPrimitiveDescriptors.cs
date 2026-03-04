using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityPrimitiveDescriptors
    {
        [MapsAnyInterface( ContextType = typeof( Ctx.Asset ) )]
        [MapsInheritingFrom( typeof( object ), ContextType = typeof( Ctx.Asset ) )]
        private static IDescriptor ProvideAsset<T>() where T : class
        {
            return new AssetDescriptor<T>();
        }
        // --- VECTORS ---

        [MapsInheritingFrom( typeof( Vector2 ) )]
        public static IDescriptor Vector2Descriptor() => new PrimitiveConfigurableDescriptor<Vector2>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 2) ? new Vector2( (float)arr[0], (float)arr[1] ) : default
        );

        [MapsInheritingFrom( typeof( Vector3 ) )]
        public static IDescriptor Vector3Descriptor() => new PrimitiveConfigurableDescriptor<Vector3>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 3) ? new Vector3( (float)arr[0], (float)arr[1], (float)arr[2] ) : default
        );

        [MapsInheritingFrom( typeof( Vector4 ) )]
        public static IDescriptor Vector4Descriptor() => new PrimitiveConfigurableDescriptor<Vector4>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z, (SerializedPrimitive)target.w },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 4) ? new Vector4( (float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3] ) : default
        );

        [MapsInheritingFrom( typeof( Vector2Int ) )]
        public static IDescriptor Vector2IntDescriptor() => new PrimitiveConfigurableDescriptor<Vector2Int>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 2) ? new Vector2Int( (int)arr[0], (int)arr[1] ) : default
        );

        [MapsInheritingFrom( typeof( Vector3Int ) )]
        public static IDescriptor Vector3IntDescriptor() => new PrimitiveConfigurableDescriptor<Vector3Int>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 3) ? new Vector3Int( (int)arr[0], (int)arr[1], (int)arr[2] ) : default
        );

        // --- QUATERNION ---

        [MapsInheritingFrom( typeof( Quaternion ) )]
        public static IDescriptor QuaternionDescriptor() => new PrimitiveConfigurableDescriptor<Quaternion>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z, (SerializedPrimitive)target.w },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 4) ? new Quaternion( (float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3] ) : default
        );
        // --- MATRIX ---

        [MapsInheritingFrom( typeof( Matrix4x4 ) )]
        public static IDescriptor Matrix4x4Descriptor() => new PrimitiveConfigurableDescriptor<Matrix4x4>(
            ( o, wrapper, ctx ) =>
            {
                wrapper.Data = new SerializedArray( 16 )
                {
                    (SerializedPrimitive)o.m00, (SerializedPrimitive)o.m01, (SerializedPrimitive)o.m02, (SerializedPrimitive)o.m03,
                    (SerializedPrimitive)o.m10, (SerializedPrimitive)o.m11, (SerializedPrimitive)o.m12, (SerializedPrimitive)o.m13,
                    (SerializedPrimitive)o.m20, (SerializedPrimitive)o.m21, (SerializedPrimitive)o.m22, (SerializedPrimitive)o.m23,
                    (SerializedPrimitive)o.m30, (SerializedPrimitive)o.m31, (SerializedPrimitive)o.m32, (SerializedPrimitive)o.m33
                };
            },
            ( data, ctx ) => new Matrix4x4()
            {
                m00 = (float)data[0],
                m01 = (float)data[1],
                m02 = (float)data[2],
                m03 = (float)data[3],
                m10 = (float)data[4],
                m11 = (float)data[5],
                m12 = (float)data[6],
                m13 = (float)data[7],
                m20 = (float)data[8],
                m21 = (float)data[9],
                m22 = (float)data[10],
                m23 = (float)data[11],
                m30 = (float)data[12],
                m31 = (float)data[13],
                m32 = (float)data[14],
                m33 = (float)data[15]
            }
        );

        // --- COLORS ---

        [MapsInheritingFrom( typeof( Color ) )]
        public static IDescriptor ColorDescriptor() => new MemberwiseDescriptor<Color>()
            .WithMember( "r", c => c.r )
            .WithMember( "g", c => c.g )
            .WithMember( "b", c => c.b )
            .WithMember( "a", c => c.a );

        [MapsInheritingFrom( typeof( Color32 ) )]
        public static IDescriptor Color32Descriptor() => new MemberwiseDescriptor<Color32>()
            .WithMember( "r", c => c.r )
            .WithMember( "g", c => c.g )
            .WithMember( "b", c => c.b )
            .WithMember( "a", c => c.a );

        // --- RECT & BOUNDS ---

        [MapsInheritingFrom( typeof( Rect ) )]
        public static IDescriptor RectDescriptor() => new MemberwiseDescriptor<Rect>()
            .WithMember( "x", r => r.x )
            .WithMember( "y", r => r.y )
            .WithMember( "width", r => r.width )
            .WithMember( "height", r => r.height );

        [MapsInheritingFrom( typeof( RectInt ) )]
        public static IDescriptor RectIntDescriptor() => new MemberwiseDescriptor<RectInt>()
            .WithMember( "x", r => r.x )
            .WithMember( "y", r => r.y )
            .WithMember( "width", r => r.width )
            .WithMember( "height", r => r.height );

        // --- BOUNDS ---

        [MapsInheritingFrom( typeof( Bounds ) )]
        public static IDescriptor BoundsDescriptor() => new MemberwiseDescriptor<Bounds>()
            .WithMember( "center", b => b.center )
            .WithMember( "extents", b => b.extents );

        [MapsInheritingFrom( typeof( BoundsInt ) )]
        public static IDescriptor BoundsIntDescriptor() => new MemberwiseDescriptor<BoundsInt>()
            .WithMember( "position", b => b.position )
            .WithMember( "size", b => b.size );

        // --- RAYS & PLANES ---

        [MapsInheritingFrom( typeof( Ray ) )]
        public static IDescriptor RayDescriptor() => new MemberwiseDescriptor<Ray>()
            .WithMember( "origin", r => r.origin )
            .WithMember( "direction", r => r.direction );

        [MapsInheritingFrom( typeof( Ray2D ) )]
        public static IDescriptor Ray2DDescriptor() => new MemberwiseDescriptor<Ray2D>()
            .WithMember( "origin", r => r.origin )
            .WithMember( "direction", r => r.direction );

        [MapsInheritingFrom( typeof( Plane ) )]
        public static IDescriptor PlaneDescriptor() => new MemberwiseDescriptor<Plane>()
            .WithMember( "normal", p => p.normal )
            .WithMember( "distance", p => p.distance );

        // --- ANIMATION / GRADIENTS ---

        [MapsInheritingFrom( typeof( Keyframe ) )]
        public static IDescriptor KeyframeDescriptor() => new MemberwiseDescriptor<Keyframe>()
            .WithMember( "time", k => k.time )
            .WithMember( "value", k => k.value )
            .WithMember( "in_tangent", k => k.inTangent )
            .WithMember( "out_tangent", k => k.outTangent )
            .WithMember( "in_weight", k => k.inWeight )
            .WithMember( "out_weight", k => k.outWeight )
            .WithMember( "weighted_mode", k => k.weightedMode );

        [MapsInheritingFrom( typeof( AnimationCurve ) )]
        public static IDescriptor AnimationCurveDescriptor() => new MemberwiseDescriptor<AnimationCurve>()
            .WithMember( "keys", c => c.keys )
            .WithMember( "pre_wrap_mode", c => c.preWrapMode )
            .WithMember( "post_wrap_mode", c => c.postWrapMode );

        [MapsInheritingFrom( typeof( GradientColorKey ) )]
        public static IDescriptor GradientColorKeyDescriptor() => new MemberwiseDescriptor<GradientColorKey>()
            .WithMember( "color", k => k.color )
            .WithMember( "time", k => k.time );

        [MapsInheritingFrom( typeof( GradientAlphaKey ) )]
        public static IDescriptor GradientAlphaKeyDescriptor() => new MemberwiseDescriptor<GradientAlphaKey>()
            .WithMember( "alpha", k => k.alpha )
            .WithMember( "time", k => k.time );

        [MapsInheritingFrom( typeof( Gradient ) )]
        public static IDescriptor GradientDescriptor() => new MemberwiseDescriptor<Gradient>()
            .WithMember( "color_keys", g => g.colorKeys )
            .WithMember( "alpha_keys", g => g.alphaKeys )
            .WithMember( "mode", g => g.mode );
    }
}