
using System;

namespace UnityPlus.Serialization
{
    public abstract class SerializationMapping
    {
        // The reason these are `object` instead of being generically typed is that when getting the mappings for `Component`
        //   `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`

        public abstract SerializedData Save( object obj, IReverseReferenceMap s );
        public abstract object Load( SerializedData data, IForwardReferenceMap l );
        public abstract void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l );

        public static SerializationMapping Empty( Type sourceType )
        {
            return (SerializationMapping)Activator.CreateInstance( typeof( EmptySerializationMapping<> ).MakeGenericType( sourceType ) );
        }

        public static SerializationMapping Empty<TSource>()
        {
            return new EmptySerializationMapping<TSource>();
        }
    }
}