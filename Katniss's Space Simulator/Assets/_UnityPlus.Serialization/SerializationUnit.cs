using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public sealed class SerializationUnit
    {
        private Saver _saver;
        private Loader _loader;

        private SerializedData[] _data;
        private object[] _objects;

        private SerializationMapping[] _mappingCache;

        public IForwardReferenceMap ForwardRefMap { get; set; }

        public IReverseReferenceMap ReverseRefMap { get; set; }

        private SerializationUnit()
        {

        }

        private void SaveCallback( IReverseReferenceMap s )
        {
            // Called by the saver.

            _data = new SerializedData[_objects.Length];

            for( int i = 0; i < _objects.Length; i++ )
            {
                object obj = _objects[i];

                if( obj == null )
                    continue;

                var mapping = SerializationMappingRegistry.GetMappingOrDefault( obj );

                _data[i] = mapping.Save( obj, s );
            }
        }

        private void PopulateCallback( IForwardReferenceMap l )
        {
            // Called by the loader.

            _mappingCache = new SerializationMapping[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                if( !data.TryGetValue( KeyNames.TYPE, out var type ) )
                    continue;

                Type type2 = type.DeserializeType();

                var mapping = SerializationMappingRegistry.GetMappingOrEmpty( type2 );
                _mappingCache[i] = mapping;

                // Parity with Member (mostly).
                object member;
                switch( mapping.SerializationStyle )
                {
                    default:
                        continue;
                    case SerializationStyle.PrimitiveStruct:
                        member = mapping.Instantiate( data, l );
                        break;
                    case SerializationStyle.NonPrimitive:
                        member = _objects[i]; // Don't instantiate when populating, object should already be created.
                        mapping.Load( ref member, data, l );
                        break;
                }

                _objects[i] = member;
            }
        }

        private void LoadCallback( IForwardReferenceMap l )
        {
            // Called by the loader.

            _objects = new object[_data.Length];
            _mappingCache = new SerializationMapping[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                if( !data.TryGetValue( KeyNames.TYPE, out var type ) )
                    continue;

                Type type2 = type.DeserializeType();

                var mapping = SerializationMappingRegistry.GetMappingOrEmpty( type2 );
                _mappingCache[i] = mapping;

                // Parity with Member.
                object member;
                switch( mapping.SerializationStyle )
                {
                    default:
                        continue;
                    case SerializationStyle.PrimitiveStruct:
                        member = mapping.Instantiate( data, l );
                        break;
                    case SerializationStyle.NonPrimitive:
                        member = mapping.Instantiate( data, l );
                        mapping.Load( ref member, data, l );
                        break;
                }

                _objects[i] = member;
            }
        }

        private void LoadReferencesCallback( IForwardReferenceMap l )
        {
            // Called by the loader.

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                var mapping = _mappingCache[i];

                object member = _objects[i];
                switch( mapping.SerializationStyle )
                {
                    default:
                        continue;
                    case SerializationStyle.PrimitiveObject:
                        member = mapping.Instantiate( data, l );
                        break;
                    case SerializationStyle.NonPrimitive:
                        mapping.LoadReferences( ref member, data, l );
                        break;
                }
                _objects[i] = member;
            }
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize()
        {
            _saver = new Saver( ReverseRefMap, SaveCallback );
            _saver.Save();
            ReverseRefMap = _saver.RefMap;
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize( IReverseReferenceMap s )
        {
            _saver = new Saver( s, SaveCallback );
            _saver.Save();
            ReverseRefMap = _saver.RefMap;
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize()
        {
            _loader = new Loader( ForwardRefMap, LoadCallback, LoadReferencesCallback );
            _loader.Load();
            ForwardRefMap = _loader.RefMap;
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            _loader = new Loader( l, LoadCallback, LoadReferencesCallback );
            _loader.Load();
            ForwardRefMap = _loader.RefMap;
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            _loader = new Loader( ForwardRefMap, PopulateCallback, LoadReferencesCallback );
            _loader.Load();
            ForwardRefMap = _loader.RefMap;
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            _loader = new Loader( l, PopulateCallback, LoadReferencesCallback );
            _loader.Load();
            ForwardRefMap = _loader.RefMap;
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the objects that were deserialized or populated.
        /// </summary>
        public IEnumerable<object> GetObjects()
        {
            return _objects;
        }

        /// <summary>
        /// Returns the objects that were deserialized or populated, but only those that are of the specified type.
        /// </summary>
        public IEnumerable<T> GetObjectsOfType<T>()
        {
            return _objects.OfType<T>();
        }

        /// <summary>
        /// Returns the data that was serialized.
        /// </summary>
        public IEnumerable<SerializedData> GetData()
        {
            return _data;
        }

        /// <summary>
        /// Returns the data that was serialized, but only of objects that are of the specified type.
        /// </summary>
        public IEnumerable<SerializedData> GetDataOfType<T>()
        {
            return _data.Where( d =>
            {
                return d.TryGetValue( KeyNames.TYPE, out var type ) && typeof( T ).IsAssignableFrom( type.DeserializeType() );
            } );
        }

        //
        //  Helper methods (unified create + act + retrieve).
        //

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
        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s )
        {
            var su = FromObjects<T>( obj );
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
            return su.GetObjectsOfType<T>().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l )
        {
            var su = FromData<T>( data );
            su.Deserialize( l );
            return su.GetObjectsOfType<T>().First();
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
        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l ) where T : class
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate( l );
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data ) where T : struct
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate();
            obj = (T)su._objects.First();
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate( l );
            obj = (T)su._objects.First();
        }

        //
        //  Creation methods (separate create + act + retrieve).
        //

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified object of type <typeparamref name="T"/>.
        /// </summary>
        public static SerializationUnit FromObjects<T>( T obj )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _objects = new object[] { obj },
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnit FromObjects( IEnumerable<object> objects )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _objects = objects.ToArray(),
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnit FromObjects( params object[] objects )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _objects = objects,
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) an object of type <typeparamref name="T"/> from the specified serialized representation.
        /// </summary>
        public static SerializationUnit FromData<T>( SerializedData data )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _data = new SerializedData[] { data },
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnit FromData( IEnumerable<SerializedData> data )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _data = data.ToArray(),
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnit FromData( params SerializedData[] data )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _data = data,
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified object of type <typeparamref name="T"/> with the specified serialized representation of the same object.
        /// </summary>
        public static SerializationUnit PopulateObject<T>( T obj, SerializedData data )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _objects = new object[] { obj },
                _data = new SerializedData[] { data },
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified objects with the corresponding specified serialized representations (objects[i] <![CDATA[<]]>==> data[i]).
        /// </summary>
        public static SerializationUnit PopulateObjects( object[] objects, SerializedData[] data )
        {
            var refMap = new BidirectionalReferenceStore();

            return new SerializationUnit()
            {
                _objects = objects,
                _data = data,
                ForwardRefMap = refMap,
                ReverseRefMap = refMap
            };
        }
    }
}