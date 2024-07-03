using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_SerializedData
    {
        [SerializationMappingProvider( typeof( SerializedPrimitive ) )]
        public static SerializationMapping SerializedPrimitiveMapping()
        {
            return new PrimitiveStructSerializationMapping<SerializedPrimitive>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedPrimitive
            };
        }

        [SerializationMappingProvider( typeof( SerializedObject ) )]
        public static SerializationMapping SerializedObjectMapping()
        {
            return new PrimitiveStructSerializationMapping<SerializedObject>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedObject
            };
        }

        [SerializationMappingProvider( typeof( SerializedArray ) )]
        public static SerializationMapping SerializedArrayMapping()
        {
            return new PrimitiveStructSerializationMapping<SerializedArray>()
            {
                OnSave = ( o, s ) => o,
                OnInstantiate = ( data, l ) => data as SerializedArray
            };
        }
    }
}