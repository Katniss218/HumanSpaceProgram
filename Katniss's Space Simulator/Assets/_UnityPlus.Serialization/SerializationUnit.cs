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

        public void Serialize()
        {
            _saver = new Saver( ReverseRefMap, SaveCallback );
            _saver.Save();
            ReverseRefMap = _saver.RefMap;
        }

        public void Deserialize()
        {
            _loader = new Loader( ForwardRefMap, LoadCallback, LoadReferencesCallback );
            _loader.Load();
            ForwardRefMap = _loader.RefMap;
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

                object obj = mapping.Load( data, l );
                _objects[i] = obj;
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

                object obj = _objects[i];
                //mapping.LoadReferences( ref _objects[i], data, l );
                mapping.LoadReferences( ref obj, data, l );
                _objects[i] = obj;
            }
        }

        public IForwardReferenceMap ForwardRefMap { get; set; }

        public IReverseReferenceMap ReverseRefMap { get; set; }


        public IEnumerable<object> GetObjects()
        {
            return _objects;
        }

        public IEnumerable<T> GetObjectsOfType<T>()
        {
            return _objects.OfType<T>();
        }

        public IEnumerable<SerializedData> GetData()
        {
            return _data;
        }

        public IEnumerable<SerializedData> GetDataOfType<T>()
        {
            return _data.Where( d =>
            {
                return d.TryGetValue( KeyNames.TYPE, out var type ) && typeof( T ).IsAssignableFrom( type.DeserializeType() );
            } );
        }

        public static SerializedData Serialize<T>( T obj )
        {
            var su = FromObjects<T>( obj );
            su.Serialize();
            return su.GetData().First();
        }

        public static T Deserialize<T>( SerializedData data )
        {
            var su = FromData<T>( data );
            su.Deserialize();
            return su.GetObjectsOfType<T>().First();
        }


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
    }
}