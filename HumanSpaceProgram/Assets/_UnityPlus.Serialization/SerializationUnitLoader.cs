using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool[] _finishedMembers;
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public IForwardReferenceMap RefMap { get; set; }

        internal SerializationUnitLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitLoader( T[] objects, SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._data = data;
            this._context = context;
        }

        public bool ShouldPause()
        {
            return false;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( int maxIters = 10 )
        {
            this._finishedMembers = new bool[_data.Length];
            this._objects = new T[_data.Length];

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback( false );
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l, int maxIters = 10 )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_data.Length];
            this._objects = new T[_data.Length];
            this.RefMap = l;

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback( false );
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( int maxIters = 10 )
        {
            this._finishedMembers = new bool[_data.Length];

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback( true );
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l, int maxIters = 10 )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_data.Length];
            this.RefMap = l;

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback( true );
                if( result != MappingResult.Progressed )
                    return;
            }
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
        public IEnumerable<TDerived> GetObjects<TDerived>()
        {
            return _objects.OfType<TDerived>();
        }

        private MappingResult LoadCallback( bool populate )
        {
            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < _data.Length; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                SerializedData data = _data[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, MappingHelper.GetSerializedType<T>( data ) );

                T member = _objects[i];
                var memberResult = mapping.SafeLoad( ref member, data, this, populate );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _finishedMembers[i] = true;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        anyProgressed = true;
                        break;
                }

                _objects[i] = member;
            }

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }
    }
}