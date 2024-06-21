using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializedDataReferenceComparer : IEqualityComparer<SerializedData>
    {
        public bool Equals( SerializedData x, SerializedData y )
        {
            return object.ReferenceEquals( x, y );
        }

        public int GetHashCode( SerializedData x )
        {
            return ((object)x).GetHashCode();
        }
    }

    public class SerializationUnitLoader<T> : ILoader
    {
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public IForwardReferenceMap RefMap { get; set; }

        public Dictionary<SerializedData, SerializationMapping> MappingCache { get; }

        internal SerializationUnitLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitLoader( T[] objects, SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._objects = objects;
            this._data = data;
            this._context = context;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize()
        {
            this.LoadCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            this.LoadCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            this.PopulateCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            this.PopulateCallback();
            this.LoadReferencesCallback();
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the objects that were deserialized or populated.
        /// </summary>
        public IEnumerable<T> GetObjects()
        {
            return _objects;
        }

        /// <summary>
        /// Returns the objects that were deserialized or populated, but only those that are of the specified type.
        /// </summary>
        public IEnumerable<Tt> GetObjectsOfType<Tt>()
        {
            return _objects.OfType<Tt>();
        }

        private void PopulateCallback()
        {
            // Called by the loader.

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                Type typeToAssignTo = data.TryGetValue( KeyNames.TYPE, out var elementType2 )
                    ? elementType2.DeserializeType()
                    : typeof( T );

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, typeToAssignTo );
                MappingCache[data] = mapping;

                // Parity with Member (mostly).
                T member = _objects[i];
                if( MappingHelper.DoPopulate( mapping, ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }

        private void LoadCallback()
        {
            // Called by the loader.

            _objects = new T[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                Type typeToAssignTo = data.TryGetValue( KeyNames.TYPE, out var elementType2 )
                    ? elementType2.DeserializeType()
                    : typeof( T );

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, typeToAssignTo );
                if( data != null )
                {
                    MappingCache[data] = mapping;
                }

                T member = default;
                if( MappingHelper.DoLoad( mapping, ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }

        private void LoadReferencesCallback()
        {
            // Called by the loader.

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                T member = _objects[i];
                SerializationMapping mapping = null;
                if( data == null )
                    SerializationMappingRegistry.GetMapping<T>( _context, member );
                else if( !MappingCache.TryGetValue( data, out mapping ) )
                {
                    SerializationMappingRegistry.GetMapping<T>( _context, member );
                }

                if( MappingHelper.DoLoadReferences( mapping, ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }
    }
}