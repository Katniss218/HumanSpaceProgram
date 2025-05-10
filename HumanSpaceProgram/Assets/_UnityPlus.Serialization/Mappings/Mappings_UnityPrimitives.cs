using System;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityPrimitives
    {
        [MapsInheritingFrom( typeof( Vector2 ) )]
        public static SerializationMapping Vector2Mapping()
        {
            return new PrimitiveSerializationMapping<Vector2>()
            {
                OnSave = ( o, s ) => new SerializedArray( 2 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnLoad = ( data, l ) => new Vector2( (float)data[0], (float)data[1] )
            };
        }

        [MapsInheritingFrom( typeof( Vector2Int ) )]
        public static SerializationMapping Vector2IntMapping()
        {
            return new PrimitiveSerializationMapping<Vector2Int>()
            {
                OnSave = ( o, s ) => new SerializedArray( 2 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnLoad = ( data, l ) => new Vector2Int( (int)data[0], (int)data[1] )
            };
        }

        [MapsInheritingFrom( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new PrimitiveSerializationMapping<Vector3>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnLoad = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [MapsInheritingFrom( typeof( Vector3Int ) )]
        public static SerializationMapping Vector3IntMapping()
        {
            return new PrimitiveSerializationMapping<Vector3Int>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnLoad = ( data, l ) => new Vector3Int( (int)data[0], (int)data[1], (int)data[2] )
            };
        }

        [MapsInheritingFrom( typeof( Vector4 ) )]
        public static SerializationMapping Vector4Mapping()
        {
            return new PrimitiveSerializationMapping<Vector4>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnLoad = ( data, l ) => new Vector4( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [MapsInheritingFrom( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new PrimitiveSerializationMapping<Quaternion>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnLoad = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [MapsInheritingFrom( typeof( Color ) )]
        public static SerializationMapping ColorMapping()
        {
            return new MemberwiseSerializationMapping<Color>()
                .WithMember( "r", o => o.r )
                .WithMember( "g", o => o.g )
                .WithMember( "b", o => o.b )
                .WithMember( "a", o => o.a );
        }

        [MapsInheritingFrom( typeof( Color32 ) )]
        public static SerializationMapping Color32Mapping()
        {
            return new MemberwiseSerializationMapping<Color32>()
                .WithMember( "r", o => o.r )
                .WithMember( "g", o => o.g )
                .WithMember( "b", o => o.b )
                .WithMember( "a", o => o.a );
        }

        [MapsInheritingFrom( typeof( Rect ) )]
        public static SerializationMapping RectMapping()
        {
            return new MemberwiseSerializationMapping<Rect>()
                .WithMember( "x", o => o.x )
                .WithMember( "y", o => o.y )
                .WithMember( "width", o => o.width )
                .WithMember( "height", o => o.height );
        }

        [MapsInheritingFrom( typeof( RectInt ) )]
        public static SerializationMapping RectIntMapping()
        {
            return new MemberwiseSerializationMapping<RectInt>()
                .WithMember( "x", o => o.x )
                .WithMember( "y", o => o.y )
                .WithMember( "width", o => o.width )
                .WithMember( "height", o => o.height );
        }

        [MapsInheritingFrom( typeof( Bounds ) )]
        public static SerializationMapping BoundsMapping()
        {
            return new MemberwiseSerializationMapping<Bounds>()
                .WithMember( "center", o => o.center )
                .WithMember( "extents", o => o.extents );
        }

        [MapsInheritingFrom( typeof( BoundsInt ) )]
        public static SerializationMapping BoundsIntMapping()
        {
            return new MemberwiseSerializationMapping<BoundsInt>()
                .WithMember( "position", o => o.position )
                .WithMember( "size", o => o.size );
        }

        [MapsInheritingFrom( typeof( Ray ) )]
        public static SerializationMapping RayMapping()
        {
            return new MemberwiseSerializationMapping<Ray>()
                .WithMember( "origin", o => o.origin )
                .WithMember( "direction", o => o.direction );
        }

        [MapsInheritingFrom( typeof( Ray2D ) )]
        public static SerializationMapping Ray2DMapping()
        {
            return new MemberwiseSerializationMapping<Ray2D>()
                .WithMember( "origin", o => o.origin )
                .WithMember( "direction", o => o.direction );
        }

        [MapsInheritingFrom( typeof( Plane ) )]
        public static SerializationMapping PlaneMapping()
        {
            return new MemberwiseSerializationMapping<Plane>()
                .WithMember( "normal", o => o.normal )
                .WithMember( "distance", o => o.distance );
        }

        [MapsInheritingFrom( typeof( GradientColorKey ) )]
        public static SerializationMapping GradientColorKeyMapping()
        {
            return new MemberwiseSerializationMapping<GradientColorKey>()
                .WithMember( "color", o => o.color )
                .WithMember( "time", o => o.time );
        }

        [MapsInheritingFrom( typeof( GradientAlphaKey ) )]
        public static SerializationMapping GradientAlphaKeyMapping()
        {
            return new MemberwiseSerializationMapping<GradientAlphaKey>()
                .WithMember( "alpha", o => o.alpha )
                .WithMember( "time", o => o.time );
        }

        [MapsInheritingFrom( typeof( Gradient ) )]
        public static SerializationMapping GradientMapping()
        {
            return new MemberwiseSerializationMapping<Gradient>()
                .WithMember( "color_keys", o => o.colorKeys )
                .WithMember( "alpha_keys", o => o.alphaKeys )
                .WithMember( "mode", o => o.mode );
        }

        [MapsInheritingFrom( typeof( Keyframe ) )]
        public static SerializationMapping KeyframeMapping()
        {
            return new MemberwiseSerializationMapping<Keyframe>()
                .WithMember( "time", o => o.time )
                .WithMember( "value", o => o.value )
                .WithMember( "in_tangent", o => o.inTangent )
                .WithMember( "out_tangent", o => o.outTangent );
        }

        [MapsInheritingFrom( typeof( AnimationCurve ) )]
        public static SerializationMapping AnimationCurveMapping()
        {
            return new MemberwiseSerializationMapping<AnimationCurve>()
                .WithMember( "keys", o => o.keys )
                .WithMember( "pre_wrap_mode", o => o.preWrapMode )
                .WithMember( "post_wrap_mode", o => o.postWrapMode );
        }

        [MapsInheritingFrom( typeof( Matrix4x4 ) )]
        public static SerializationMapping Matrix4x4Mapping()
        {
            return new PrimitiveSerializationMapping<Matrix4x4>()
            {
                OnSave = ( o, s ) => new SerializedArray( 16 )
                {
                    (SerializedPrimitive)o.m00, (SerializedPrimitive)o.m01, (SerializedPrimitive)o.m02, (SerializedPrimitive)o.m03,
                    (SerializedPrimitive)o.m10, (SerializedPrimitive)o.m11, (SerializedPrimitive)o.m12, (SerializedPrimitive)o.m13,
                    (SerializedPrimitive)o.m20, (SerializedPrimitive)o.m21, (SerializedPrimitive)o.m22, (SerializedPrimitive)o.m23,
                    (SerializedPrimitive)o.m30, (SerializedPrimitive)o.m31, (SerializedPrimitive)o.m32, (SerializedPrimitive)o.m33
                },
                OnLoad = ( data, l ) => new Matrix4x4()
                {
                    m00 = (float)data[0],
                    m01 = (float)data[1],
                    m02 = (float)data[2],
                    m03 = (float)data[3],
                    m10 = (float)data[0],
                    m11 = (float)data[1],
                    m12 = (float)data[2],
                    m13 = (float)data[3],
                    m20 = (float)data[0],
                    m21 = (float)data[1],
                    m22 = (float)data[2],
                    m23 = (float)data[3],
                    m30 = (float)data[0],
                    m31 = (float)data[1],
                    m32 = (float)data[2],
                    m33 = (float)data[3]
                }
            };
        }
    }
}