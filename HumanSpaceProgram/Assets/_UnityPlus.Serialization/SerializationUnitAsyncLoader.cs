using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UnityPlus.Serialization
{
    public class SerializationUnitAsyncLoader<T> : ILoader
    {
        private bool[] _finishedMembers;
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public long AllowedMilisecondsPerInvocation { get; set; } = 100;
        long _lastInvocationTimestamp = 0;

        public IForwardReferenceMap RefMap { get; set; }
        public MappingResult Result { get; private set; }

        internal SerializationUnitAsyncLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitAsyncLoader( T[] objects, SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._data = data;
            this._context = context;
        }

        public bool ShouldPause()
        {
            long stamp = Stopwatch.GetTimestamp();
            long miliseconds = ((stamp - _lastInvocationTimestamp) * 1000) / Stopwatch.Frequency;

            return (miliseconds > AllowedMilisecondsPerInvocation);
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize()
        {
            this._finishedMembers = new bool[_data.Length];
            this._objects = new T[_data.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( false );
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_data.Length];
            this._objects = new T[_data.Length];
            this.RefMap = l;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( false );
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            this._finishedMembers = new bool[_data.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( true );
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_data.Length];
            this.RefMap = l;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( true );
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