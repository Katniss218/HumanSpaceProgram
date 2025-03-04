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

        [MapsInheritingFrom( typeof( Matrix4x4 ) )]
        public static SerializationMapping Matrix4x4Mapping()
        {
            return new PrimitiveSerializationMapping<Matrix4x4>()
            {
                OnSave = ( o, s ) => new SerializedArray( 16 ) 
                { 
                    (SerializedPrimitive)o.m00, (SerializedPrimitive)o.m01, (SerializedPrimitive)o.m02 , (SerializedPrimitive)o.m03,
                    (SerializedPrimitive)o.m10, (SerializedPrimitive)o.m11, (SerializedPrimitive)o.m12 , (SerializedPrimitive)o.m13,
                    (SerializedPrimitive)o.m20, (SerializedPrimitive)o.m21, (SerializedPrimitive)o.m22 , (SerializedPrimitive)o.m23,
                    (SerializedPrimitive)o.m30, (SerializedPrimitive)o.m31, (SerializedPrimitive)o.m32 , (SerializedPrimitive)o.m33
                },
                OnLoad = ( data, l ) => new Matrix4x4()
                {
                     m00 = (float)data[0], m01 = (float)data[1], m02 = (float)data[2], m03 = (float)data[3],
                     m10 = (float)data[0], m11 = (float)data[1], m12 = (float)data[2], m13 = (float)data[3],
                     m20 = (float)data[0], m21 = (float)data[1], m22 = (float)data[2], m23 = (float)data[3],
                     m30 = (float)data[0], m31 = (float)data[1], m32 = (float)data[2], m33 = (float)data[3]
                }
            };
        }
    }
}