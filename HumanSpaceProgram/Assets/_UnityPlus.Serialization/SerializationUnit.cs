using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public static partial class SerializationUnit
    {
        /// <summary>
        /// Helper method to serialize a single object easily.
        /// </summary>
        public static SerializedData Serialize<T>( T obj )
        {
            var su = FromObjects<T>( obj );
            su.Serialize();
            return su.GetData().First();
        }

        /// <summary>
        /// Helper method to serialize a single object easily.
        /// </summary>
        public static SerializedData Serialize<T>( int context, T obj )
        {
            var su = FromObjects<T>( context, obj );
            su.Serialize();
            return su.GetData().First();
        }

        /// <summary>
        /// Helper method to serialize a single object easily.
        /// </summary>
        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s )
        {
            var su = FromObjects<T>( obj );
            su.Serialize( s );
            return su.GetData().First();
        }
        
        /// <summary>
        /// Helper method to serialize a single object easily.
        /// </summary>
        public static SerializedData Serialize<T>( int context, T obj, IReverseReferenceMap s )
        {
            var su = FromObjects<T>( context, obj );
            su.Serialize( s );
            return su.GetData().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( SerializedData data )
        {
            var su = FromData<T>( data );
            su.Deserialize();
            return su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( int context, SerializedData data )
        {
            var su = FromData<T>( context, data );
            su.Deserialize();
            return su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l )
        {
            var su = FromData<T>( data );
            su.Deserialize( l );
            return su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( int context, SerializedData data, IForwardReferenceMap l )
        {
            var su = FromData<T>( context, data );
            su.Deserialize( l );
            return su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to populate the members of a single object easily.
        /// </summary>
        public static void Populate<T>( T obj, SerializedData data ) where T : class
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate();
        }

        /// <summary>
        /// Helper method to populate the members of a single object easily.
        /// </summary>
        public static void Populate<T>( int context, T obj, SerializedData data ) where T : class
        {
            var su = PopulateObject<T>( context, obj, data );
            su.Populate();
        }

        /// <summary>
        /// Helper method to populate the members of a single object easily.
        /// </summary>
        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l ) where T : class
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate( l );
        }

        /// <summary>
        /// Helper method to populate the members of a single object easily.
        /// </summary>
        public static void Populate<T>( int context, T obj, SerializedData data, IForwardReferenceMap l ) where T : class
        {
            var su = PopulateObject<T>( context, obj, data );
            su.Populate( l );
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data ) where T : struct
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate();
            obj = su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( int context, ref T obj, SerializedData data ) where T : struct
        {
            var su = PopulateObject<T>( context, obj, data );
            su.Populate();
            obj = su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate( l );
            obj = su.GetObjects().First();
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( int context, ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
        {
            var su = PopulateObject<T>( context, obj, data );
            su.Populate( l );
            obj = su.GetObjects().First();
        }

        //
        //  Creation methods (separate create + act + retrieve).
        //

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified object of type <typeparamref name="T"/>.
        /// </summary>
        public static SerializationUnitSaver<T> FromObjects<T>( T obj )
        {
            return new SerializationUnitSaver<T>( new T[] { obj }, ObjectContext.Default );
        }

        public static SerializationUnitSaver<T> FromObjects<T>( int context, T obj )
        {
            return new SerializationUnitSaver<T>( new T[] { obj }, context );
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnitSaver<T> FromObjects<T>( IEnumerable<T> objects )
        {
            return new SerializationUnitSaver<T>( objects.ToArray(), ObjectContext.Default );
        }

        public static SerializationUnitSaver<T> FromObjects<T>( int context, IEnumerable<T> objects )
        {
            return new SerializationUnitSaver<T>( objects.ToArray(), context );
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnitSaver<T> FromObjects<T>( params T[] objects )
        {
            return new SerializationUnitSaver<T>( objects, ObjectContext.Default );
        }

        public static SerializationUnitSaver<T> FromObjects<T>( int context, params T[] objects )
        {
            return new SerializationUnitSaver<T>( objects, context );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) an object of type <typeparamref name="T"/> from the specified serialized representation.
        /// </summary>
        public static SerializationUnitLoader<T> FromData<T>( SerializedData data )
        {
            return new SerializationUnitLoader<T>( new SerializedData[] { data }, ObjectContext.Default );
        }
        public static SerializationUnitLoader<T> FromData<T>( int context, SerializedData data )
        {
            return new SerializationUnitLoader<T>( new SerializedData[] { data }, context );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnitLoader<T> FromData<T>( IEnumerable<SerializedData> data )
        {
            return new SerializationUnitLoader<T>( data.ToArray(), ObjectContext.Default );
        }
        public static SerializationUnitLoader<T> FromData<T>( int context, IEnumerable<SerializedData> data )
        {
            return new SerializationUnitLoader<T>( data.ToArray(), context );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnitLoader<T> FromData<T>( params SerializedData[] data )
        {
            return new SerializationUnitLoader<T>( data, ObjectContext.Default );
        }
        public static SerializationUnitLoader<T> FromData<T>( int context, params SerializedData[] data )
        {
            return new SerializationUnitLoader<T>( data, context );
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified object of type <typeparamref name="T"/> with the specified serialized representation of the same object.
        /// </summary>
        public static SerializationUnitLoader<T> PopulateObject<T>( T obj, SerializedData data )
        {
            return new SerializationUnitLoader<T>( new T[] { obj }, new SerializedData[] { data }, ObjectContext.Default );
        }

        public static SerializationUnitLoader<T> PopulateObject<T>( int context, T obj, SerializedData data )
        {
            return new SerializationUnitLoader<T>( new T[] { obj }, new SerializedData[] { data }, context );
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified objects with the corresponding specified serialized representations (objects[i] <![CDATA[<]]>==> data[i]).
        /// </summary>
        public static SerializationUnitLoader<T> PopulateObjects<T>( T[] objects, SerializedData[] data )
        {
            return new SerializationUnitLoader<T>( objects, data, ObjectContext.Default );
        }
        public static SerializationUnitLoader<T> PopulateObjects<T>( int context, T[] objects, SerializedData[] data )
        {
            return new SerializationUnitLoader<T>( objects, data, context );
        }
    }
}