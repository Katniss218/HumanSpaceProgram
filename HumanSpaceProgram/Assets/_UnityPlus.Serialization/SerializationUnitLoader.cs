using System;
using System.Collections.Generic;
using System.Linq;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitLoader<T> : ILoader
    {
        private SerializedData[] _data;
        private T[] _objects;
        int _startIndex;
        Dictionary<int, RetryEntry<T>> _retryElements;
        bool _wasFailureNoRetry;

        private int _context = default;

        public IForwardReferenceMap RefMap { get; set; }

        public int CurrentPass { get; private set; }

        internal SerializationUnitLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitLoader( T[] objects, SerializedData[] data, int context )
        {
            if( objects.Length != data.Length )
                throw new ArgumentException( $"The length of '{nameof( objects )}' ({objects.Length}) must match the length of '{nameof( data )}' ({data.Length})." );

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
        public SerializationResult Deserialize( int maxIters = 10 )
        {
            if( maxIters <= 0 )
                throw new ArgumentOutOfRangeException( nameof( maxIters ), $"The maximum number of iterations must be greater than zero." );

            this._objects = new T[_data.Length];
            this.CurrentPass = -1;

            SerializationResult result = SerializationResult.NoChange;
            for( int i = 0; i < maxIters; i++ )
            {
                this.CurrentPass++;
                result = this.LoadOrPopulateCallback( false );
                if( result.HasFlag( SerializationResult.Finished ) )
                    return result;
            }

            return result;
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public SerializationResult Deserialize( IForwardReferenceMap l, int maxIters = 10 )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );
            if( maxIters <= 0 )
                throw new ArgumentOutOfRangeException( nameof( maxIters ), $"The maximum number of iterations must be greater than zero." );

            this._objects = new T[_data.Length];
            this.CurrentPass = -1;
            this.RefMap = l;

            SerializationResult result = SerializationResult.NoChange;
            for( int i = 0; i < maxIters; i++ )
            {
                this.CurrentPass++;
                result = this.LoadOrPopulateCallback( false );
                if( result.HasFlag( SerializationResult.Finished ) )
                    return result;
            }

            return result;
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public SerializationResult Populate( int maxIters = 10 )
        {
            if( maxIters <= 0 )
                throw new ArgumentOutOfRangeException( nameof( maxIters ), $"The maximum number of iterations must be greater than zero." );
            this.CurrentPass = -1;

            SerializationResult result = SerializationResult.NoChange;
            for( int i = 0; i < maxIters; i++ )
            {
                this.CurrentPass++;
                result = this.LoadOrPopulateCallback( true );
                if( result.HasFlag( SerializationResult.Finished ) )
                    return result;
            }

            return result;
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public SerializationResult Populate( IForwardReferenceMap l, int maxIters = 10 )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );
            if( maxIters <= 0 )
                throw new ArgumentOutOfRangeException( nameof( maxIters ), $"The maximum number of iterations must be greater than zero." );

            this.CurrentPass = -1;
            this.RefMap = l;

            SerializationResult result = SerializationResult.NoChange;
            for( int i = 0; i < maxIters; i++ )
            {
                this.CurrentPass++;
                result = this.LoadOrPopulateCallback( true );
                if( result.HasFlag( SerializationResult.Finished ) )
                    return result;
            }
            return result;
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

        private SerializationResult LoadOrPopulateCallback( bool populate )
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

                    SerializationResult elementResult = mapping.SafeLoad( ref obj, data, this, populate );
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = CurrentPass;
                    }
                    else if( elementResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    _objects[i] = obj;
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryElements.Remove( i );
                }
            }

            for( int i = _startIndex; i < _data.Length; i++ )
            {
                T obj = _objects[i];
                SerializedData data = _data[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, MappingHelper.GetSerializedType<T>( data ) );

                SerializationResult elementResult = mapping.SafeLoad( ref obj, data, this, populate );
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

                _objects[i] = obj;
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