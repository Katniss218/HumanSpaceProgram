using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_References
    {
        [SerializationMappingProvider( typeof( object ), Context = ObjectContext.Ref )]
        public static SerializationMapping ObjectRefMapping<T>() where T : class
        {
            return new PrimitiveStructSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteObjectReference<T>( o ),
                OnInstantiate = ( data, l ) => l.ReadObjectReference<T>( data )
            };
        }

        [SerializationMappingProvider( typeof( Array ), Context = ObjectContext.Ref )]
        public static SerializationMapping ArrayReferenceMapping<T>() where T : class
        {
            return new PrimitiveStructSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        var data = s.RefMap.WriteObjectReference<T>( o[i] );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    SerializedArray serializedArray = (SerializedArray)data;

                    T[] array = new T[serializedArray.Count];

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        var element = l.ReadObjectReference<T>( elementData );
                        array[i] = element;
                    }

                    return array;
                }
            };
        }

#warning TODO - generic mappings might want to be used on different types of things, kind of like the generic constraints. This method here is currently not safe because it can be invoked on a struct.

    }
}