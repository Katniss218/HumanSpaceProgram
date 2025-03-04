using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitAsyncSaver<T> : ISaver
    {
        private SerializedData[] _data;
        private T[] _objects;
        int _startIndex;
        Dictionary<int, RetryEntry<T>> _retryElements;
        bool _wasFailureNoRetry;

        private int _context = default;

        public long AllowedMilisecondsPerInvocation { get; set; } = 100;
        long _lastInvocationTimestamp = 0;

        public IReverseReferenceMap RefMap { get; set; }

        public int CurrentPass { get; private set; }

        public SerializationResult Result { get; private set; }

        internal SerializationUnitAsyncSaver( T[] objects, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.CurrentPass = -1;
            this._objects = objects;
            this._data = new SerializedData[objects.Length];
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
            //this._data = new SerializedData[_objects.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.SaveCallback();
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize( IReverseReferenceMap s )
        {
            if( s == null )
                throw new ArgumentNullException( nameof( s ), $"The reference map to use can't be null." );

            //this._data = new SerializedData[_objects.Length];
            this.RefMap = s;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.SaveCallback();
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

        SerializationResult _lastResult;

        private void SaveCallback()
        {
#warning TODO - pass doesn't increment correctly when pausing.
#warning TODO - mappings don't respond correctly to paused state.

            //if( _lastResult != SerializationResult.Paused && this.Result != SerializationResult.Paused )
            this.CurrentPass++;

            if( _retryElements != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryElements )
                {
                    if( entry.pass == CurrentPass )
                        continue;

                    T obj = _objects[i];
                    SerializedData data = _data[i];

                    var mapping = entry.mapping;

                    _lastResult = mapping.SafeSave<T>( obj, ref data, this );
                    if( _lastResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = CurrentPass;
                    }
                    else if( _lastResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    _data[i] = data;

                    if( this.ShouldPause() )
                    {
                        foreach( var ii in retryMembersThatSucceededThisTime )
                        {
                            _retryElements.Remove( ii );
                        }

                        this.Result = SerializationResult.Paused;
                        return;
                    }
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryElements.Remove( i );
                }
            }

            for( int i = _startIndex; i < _objects.Length; i++ )
            {
                T obj = _objects[i];
                SerializedData data = _data[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, obj );

                _lastResult = mapping.SafeSave<T>( obj, ref data, this );
                if( _lastResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( _lastResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;

                    _startIndex = i + 1;
                }
                else
                {
                    _retryElements ??= new();
                    _startIndex = i + 1;

                    if( _lastResult.HasFlag( SerializationResult.Paused ) )
                        _retryElements.Add( i, new RetryEntry<T>( obj, mapping, -1 ) );
                    else
                        _retryElements.Add( i, new RetryEntry<T>( obj, mapping, CurrentPass ) );
                }

                _data[i] = data;

                if( this.ShouldPause() )
                {
                    this.Result = SerializationResult.Paused;
                    return;
                }
            }

            SerializationResult result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryElements != null && _retryElements.Count != 0 )
                result |= SerializationResult.HasFailures;
            if( _retryElements == null || _retryElements.Count == 0 )
                result |= SerializationResult.Finished;

            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;

            this.Result = result;
        }
    }
}