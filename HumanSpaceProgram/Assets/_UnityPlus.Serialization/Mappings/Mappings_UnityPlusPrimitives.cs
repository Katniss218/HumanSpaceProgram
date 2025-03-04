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
    }
}