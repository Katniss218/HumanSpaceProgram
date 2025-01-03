
namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_SerializedData
    {
        [MapsInheritingFrom( typeof( SerializedPrimitive ) )]
        public static SerializationMapping SerializedPrimitiveMapping()
        {
            return new PrimitiveSerializationMapping<SerializedPrimitive>()
            {
                OnSave = ( o, s ) => o,
                OnLoad = ( data, l ) => data as SerializedPrimitive
            };
        }

        [MapsInheritingFrom( typeof( SerializedObject ) )]
        public static SerializationMapping SerializedObjectMapping()
        {
            return new PrimitiveSerializationMapping<SerializedObject>()
            {
                OnSave = ( o, s ) => o,
                OnLoad = ( data, l ) => data as SerializedObject
            };
        }

        [MapsInheritingFrom( typeof( SerializedArray ) )]
        public static SerializationMapping SerializedArrayMapping()
        {
            return new PrimitiveSerializationMapping<SerializedArray>()
            {
                OnSave = ( o, s ) => o,
                OnLoad = ( data, l ) => data as SerializedArray
            };
        }
    }
}