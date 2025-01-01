using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitAsyncSaver<T> : ISaver
    {
        private bool[] _finishedMembers;
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public long AllowedMilisecondsPerInvocation { get; set; } = 100;
        long _lastInvocationTimestamp = 0;

        public IReverseReferenceMap RefMap { get; set; }
        public MappingResult Result { get; private set; }

        internal SerializationUnitAsyncSaver( T[] objects, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
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
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize()
        {
            this._finishedMembers = new bool[_objects.Length];
            this._data = new SerializedData[_objects.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.SaveCallback();
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize( IReverseReferenceMap s )
        {
            if( s == null )
                throw new ArgumentNullException( nameof( s ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_objects.Length];
            this._data = new SerializedData[_objects.Length];
            this.RefMap = s;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.SaveCallback();
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the data that was serialized.
        /// </summary>
        public IEnumerable<SerializedData> GetData()
        {
            if( _data == null )
                throw new InvalidOperationException( $"Can't get the saved data before any has been saved." );

            return _data;
        }

        /// <summary>
        /// Returns the data that was serialized, but only of objects that are of the specified type.
        /// </summary>
        public IEnumerable<SerializedData> GetDataOfType<TDerived>()
        {
            if( _data == null )
                throw new InvalidOperationException( $"Can't get the saved data before any has been saved." );

            return _data.Where( d =>
            {
                return d.TryGetValue( KeyNames.TYPE, out var type ) && typeof( TDerived ).IsAssignableFrom( type.DeserializeType() );
            } );
        }

        private MappingResult SaveCallback()
        {
            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < _objects.Length; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                T obj = _objects[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, obj );

                var data = _data[i];
                MappingResult memberResult = mapping.SafeSave<T>( obj, ref data, this );
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

                _data[i] = data;
            }

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }
    }
}