using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_References
    {
        [MapsAnyInterface( Context = ObjectContext.Ref )]
        [MapsAnyClass( Context = ObjectContext.Ref )]
        public static SerializationMapping ObjectRefMapping<T>() where T : class
        {
            return new PrimitiveStructSerializationMapping<T>()
            {
                OnSave = ( o, s ) => s.RefMap.WriteObjectReference<T>( o ),
                OnInstantiate = ( data, l ) => l.ReadObjectReference<T>( data )
            };
        }

        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Refs )]
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
    }
}