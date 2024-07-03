using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityPrimitives
    {
        [SerializationMappingProvider( typeof( Vector2 ) )]
        public static SerializationMapping Vector2Mapping()
        {
            return new PrimitiveStructSerializationMapping<Vector2>()
            {
                OnSave = ( o, s ) => new SerializedArray( 2 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnInstantiate = ( data, l ) => new Vector2( (float)data[0], (float)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector2Int ) )]
        public static SerializationMapping Vector2IntMapping()
        {
            return new PrimitiveStructSerializationMapping<Vector2Int>()
            {
                OnSave = ( o, s ) => new SerializedArray( 2 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y },
                OnInstantiate = ( data, l ) => new Vector2Int( (int)data[0], (int)data[1] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new PrimitiveStructSerializationMapping<Vector3>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector3Int ) )]
        public static SerializationMapping Vector3IntMapping()
        {
            return new PrimitiveStructSerializationMapping<Vector3Int>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3Int( (int)data[0], (int)data[1], (int)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Vector4 ) )]
        public static SerializationMapping Vector4Mapping()
        {
            return new PrimitiveStructSerializationMapping<Vector4>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new Vector4( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new PrimitiveStructSerializationMapping<Quaternion>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }
    }
}