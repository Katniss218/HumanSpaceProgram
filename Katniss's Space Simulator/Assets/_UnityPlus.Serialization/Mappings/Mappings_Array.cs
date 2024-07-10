using System;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Array
    {
        [MapsInheritingFrom( typeof( Array ), Context = ArrayContext.Values )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new NonPrimitiveSerializationMapping<T[]>()
            {
                OnSave = ( o, s ) =>
                {
                    if( o == null )
                        return null;

                    SerializedArray serializedArray = new SerializedArray( o.Length );
                    for( int i = 0; i < o.Length; i++ )
                    {
                        T value = o[i];

                        var mapping = SerializationMappingRegistry.GetMapping<T>( ObjectContext.Default, value );
                        var data = mapping.SafeSave<T>( value, s );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                OnInstantiate = ( data, l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return null;

                    return data == null ? default : new T[serializedArray.Count];
                },
                OnLoad = ( ref T[] o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        Type elementType = elementData != null && elementData.TryGetValue( KeyNames.TYPE, out var elementType2 )
                            ? elementType2.DeserializeType()
                            : typeof( T );

                        T element = default;
                        var mapping = MappingHelper.GetMapping_Load<T>( ObjectContext.Default, elementType, elementData, l );
                        if( mapping.SafeLoad( ref element, elementData, l ) )
                        {
                            o[i] = element;
                        }
                    }
                },
                OnLoadReferences = ( ref T[] o, SerializedData data, ILoader l ) =>
                {
                    if( data is not SerializedArray serializedArray )
                        return;

                    for( int i = 0; i < o.Length; i++ )
                    {
                        SerializedData elementData = serializedArray[i];

                        T element = o[i];
                        var mapping = MappingHelper.GetMapping_LoadReferences<T>( ObjectContext.Default, element, elementData, l );
                        if( mapping.SafeLoadReferences( ref element, elementData, l ) )
                        {
                            o[i] = element;
                        }
                    }
                }
            };
        }
    }
}