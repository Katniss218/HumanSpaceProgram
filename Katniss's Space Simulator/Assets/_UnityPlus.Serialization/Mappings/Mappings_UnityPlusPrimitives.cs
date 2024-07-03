using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_UnityPlusPrimitives
    {
        [SerializationMappingProvider( typeof( Vector3Dbl ) )]
        public static SerializationMapping Vector3DblMapping()
        {
            return new PrimitiveStructSerializationMapping<Vector3Dbl>()
            {
                OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                OnInstantiate = ( data, l ) => new Vector3Dbl( (double)data[0], (double)data[1], (double)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( QuaternionDbl ) )]
        public static SerializationMapping QuaternionDblMapping()
        {
            return new PrimitiveStructSerializationMapping<QuaternionDbl>()
            {
                OnSave = ( o, s ) => new SerializedArray( 4 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                OnInstantiate = ( data, l ) => new QuaternionDbl( (double)data[0], (double)data[1], (double)data[2], (double)data[3] )
            };
        }
    }
}