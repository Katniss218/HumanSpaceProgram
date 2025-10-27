using System;
using System.Collections.Generic;
using System.Linq;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitSaver<T> : ISaver
    {
        private SerializedData[] _data;
        private T[] _objects;
        int _startIndex;
        Dictionary<int, RetryEntry<T>> _retryElements;
        bool _wasFailureNoRetry;

        private int _context = default;

        public IReverseReferenceMap RefMap { get; set; }

        public int CurrentPass { get; private set; }


        internal SerializationUnitSaver( T[] objects, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
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
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public SerializationResult Serialize( int maxIters = 10 )
        {
            if( maxIters <= 0 )
                throw new ArgumentOutOfRangeException( nameof( maxIters ), $"The maximum number of iterations must be greater than zero." );

            this._data = new SerializedData[_objects.Length];
            this.CurrentPass = -1;

            SerializationResult result = SerializationResult.NoChange;
            for( int i = 0; i < maxIters; i++ )
            {
                this.CurrentPass++;
                result = this.SaveCallback();
                if( result.HasFlag( SerializationResult.Finished ) )
                    return result;
            }
            return result;
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public SerializationResult Serialize( IReverseReferenceMap s, int maxIters = 10 )
        {
            if( s == null )
                throw new ArgumentNullException( nameof( s ), $"The reference map to use can't be null." );
            if( maxIters <= 0 )
                throw new ArgumentOutOfRangeException( nameof( maxIters ), $"The maximum number of iterations must be greater than zero." );

            this._data = new SerializedData[_objects.Length];
            this.CurrentPass = -1;
            this.RefMap = s;

            SerializationResult result = SerializationResult.NoChange;
            for( int i = 0; i < maxIters; i++ )
            {
                this.CurrentPass++;
                result = this.SaveCallback();
                if( result.HasFlag( SerializationResult.Finished ) )
                    return result;
            }
            return result;
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

        private SerializationResult SaveCallback()
        {
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

                    SerializationResult elementResult = mapping.SafeSave<T>( obj, ref data, this );
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = CurrentPass;
                    }
                    else if( elementResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    _data[i] = data;
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

                SerializationResult elementResult = mapping.SafeSave<T>( obj, ref data, this );
                if( elementResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;

                    _startIndex = i + 1;
                }
                else
                {
                    _retryElements ??= new();
                    _startIndex = i + 1;

                    if( elementResult.HasFlag( SerializationResult.Paused ) )
                        _retryElements.Add( i, new RetryEntry<T>( obj, mapping, -1 ) );
                    else
                        _retryElements.Add( i, new RetryEntry<T>( obj, mapping, CurrentPass ) );
                }

                _data[i] = data;
            }

            SerializationResult result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryElements != null && _retryElements.Count != 0 )
                result |= SerializationResult.HasFailures;
            if( _retryElements == null || _retryElements.Count == 0 )
                result |= SerializationResult.Finished;

            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;

            return result;
        }
    }
}