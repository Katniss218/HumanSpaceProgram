using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public static partial class SerializationUnit
    {
        //
        //  Creation methods (separate create + act + retrieve).
        //

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified object of type <typeparamref name="T"/>.
        /// </summary>
        public static SerializationUnitAsyncSaver<T> FromObjectsAsync<T>( T obj )
        {
            return new SerializationUnitAsyncSaver<T>( new T[] { obj }, ObjectContext.Default );
        }

        public static SerializationUnitAsyncSaver<T> FromObjectsAsync<T>( int context, T obj )
        {
            return new SerializationUnitAsyncSaver<T>( new T[] { obj }, context );
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnitAsyncSaver<T> FromObjectsAsync<T>( IEnumerable<T> objects )
        {
            return new SerializationUnitAsyncSaver<T>( objects.ToArray(), ObjectContext.Default );
        }

        public static SerializationUnitAsyncSaver<T> FromObjectsAsync<T>( int context, IEnumerable<T> objects )
        {
            return new SerializationUnitAsyncSaver<T>( objects.ToArray(), context );
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnitAsyncSaver<T> FromObjectsAsync<T>( params T[] objects )
        {
            return new SerializationUnitAsyncSaver<T>( objects, ObjectContext.Default );
        }

        public static SerializationUnitAsyncSaver<T> FromObjectsAsync<T>( int context, params T[] objects )
        {
            return new SerializationUnitAsyncSaver<T>( objects, context );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) an object of type <typeparamref name="T"/> from the specified serialized representation.
        /// </summary>
        public static SerializationUnitAsyncLoader<T> FromDataAsync<T>( SerializedData data )
        {
            return new SerializationUnitAsyncLoader<T>( new SerializedData[] { data }, ObjectContext.Default );
        }
        public static SerializationUnitAsyncLoader<T> FromDataAsync<T>( int context, SerializedData data )
        {
            return new SerializationUnitAsyncLoader<T>( new SerializedData[] { data }, context );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnitAsyncLoader<T> FromDataAsync<T>( IEnumerable<SerializedData> data )
        {
            return new SerializationUnitAsyncLoader<T>( data.ToArray(), ObjectContext.Default );
        }
        public static SerializationUnitAsyncLoader<T> FromDataAsync<T>( int context, IEnumerable<SerializedData> data )
        {
            return new SerializationUnitAsyncLoader<T>( data.ToArray(), context );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnitAsyncLoader<T> FromDataAsync<T>( params SerializedData[] data )
        {
            return new SerializationUnitAsyncLoader<T>( data, ObjectContext.Default );
        }
        public static SerializationUnitAsyncLoader<T> FromDataAsync<T>( int context, params SerializedData[] data )
        {
            return new SerializationUnitAsyncLoader<T>( data, context );
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified object of type <typeparamref name="T"/> with the specified serialized representation of the same object.
        /// </summary>
        public static SerializationUnitAsyncLoader<T> PopulateObjectAsync<T>( T obj, SerializedData data )
        {
            return new SerializationUnitAsyncLoader<T>( new T[] { obj }, new SerializedData[] { data }, ObjectContext.Default );
        }

        public static SerializationUnitAsyncLoader<T> PopulateObjectAsync<T>( int context, T obj, SerializedData data )
        {
            return new SerializationUnitAsyncLoader<T>( new T[] { obj }, new SerializedData[] { data }, context );
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified objects with the corresponding specified serialized representations (objects[i] <![CDATA[<]]>==> data[i]).
        /// </summary>
        public static SerializationUnitAsyncLoader<T> PopulateObjectsAsync<T>( T[] objects, SerializedData[] data )
        {
            return new SerializationUnitAsyncLoader<T>( objects, data, ObjectContext.Default );
        }
        public static SerializationUnitAsyncLoader<T> PopulateObjectsAsync<T>( int context, T[] objects, SerializedData[] data )
        {
            return new SerializationUnitAsyncLoader<T>( objects, data, context );
        }
    }
}