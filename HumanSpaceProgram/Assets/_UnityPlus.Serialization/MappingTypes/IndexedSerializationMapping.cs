using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A type of mapping that operates on a collection type that's indexable by an integer.
    /// </summary>
    /// <typeparam name="TSource">The type being mapped.</typeparam>
    public class IndexedSerializationMapping<TSource, TElement> : SerializationMapping
    {
        int elementContext;
        Func<TSource, int> elementCountGetter;
        Func<TSource, int, TElement> elementGetter;
        Action<TSource, int, TElement> elementSetter;
        private bool _objectHasBeenInstantiated;

        TElement[] _factoryElementStorage;
        int _startIndex;
        Dictionary<int, RetryEntry<TElement>> _retryElements;

        Func<SerializedData, ILoader, object> _rawFactory = null;
        Func<int, object> _factory;
        Func<IEnumerable<TElement>, object> _lateFactory;

        bool _wasFailureNoRetry = false;

        public IndexedSerializationMapping( Func<TSource, int> countGetter, Func<TSource, int, TElement> getter, Action<TSource, int, TElement> setter )
        {
            elementContext = 0;
            this.elementCountGetter = countGetter;
            this.elementGetter = getter;
            this.elementSetter = setter;
        }

        public IndexedSerializationMapping( Func<TSource, int> countGetter, int elementContext, Func<TSource, int, TElement> getter, Action<TSource, int, TElement> setter )
        {
            this.elementContext = elementContext;
            this.elementCountGetter = countGetter;
            this.elementGetter = getter;
            this.elementSetter = setter;
        }

        public override SerializationMapping GetInstance()
        {
            return new IndexedSerializationMapping<TSource, TElement>( elementCountGetter, elementContext, elementGetter, elementSetter )
            {
                Context = Context,
                _factory = _factory,
                _lateFactory = _lateFactory
            };
        }

        public override SerializationResult Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return SerializationResult.Finished;
            }

            TSource sourceObj = (TSource)(object)obj;
            int length = elementCountGetter.Invoke( sourceObj );

            SerializedArray serArray;
            if( data == null )
            {
                data = new SerializedObject();
                var headerStyle = MappingHelper.IsNonNullEligibleForTypeHeader<TMember>();
                if( headerStyle != ObjectHeaderStyle.None )
                {
                    if( headerStyle.HasFlag( ObjectHeaderStyle.TypeField ) )
                        data[KeyNames.TYPE] = obj.GetType().SerializeType();
                    if( headerStyle.HasFlag( ObjectHeaderStyle.IDField ) )
                        data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                }

                serArray = new SerializedArray( length );
                data["value"] = serArray;
            }
            else
            {
                serArray = (SerializedArray)data["value"];
            }

            //
            //
            //

            if( _retryElements != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryElements )
                {
                    SerializedData elementData = null;

                    SerializationResult elementResult = entry.mapping.SafeSave<TElement>( entry.value, ref elementData, s );
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = s.CurrentPass;
                    }
                    else if( elementResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    serArray[i] = elementData;

                    if( s.ShouldPause() )
                    {
                        foreach( var ii in retryMembersThatSucceededThisTime )
                        {
                            _retryElements.Remove( ii );
                        }
                        return SerializationResult.Paused;
                    }
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryElements.Remove( i );
                }
            }

            //
            //
            //
            for( int i = _startIndex; i < length; i++ )
            {
                TElement elementObj = elementGetter.Invoke( sourceObj, i );

                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, elementObj );

                SerializedData elementData = null;

                SerializationResult elementResult = mapping.SafeSave( elementObj, ref elementData, s );
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
                        _retryElements.Add( i, new RetryEntry<TElement>( elementObj, mapping, -1 ) );
                    else
                        _retryElements.Add( i, new RetryEntry<TElement>( elementObj, mapping, s.CurrentPass ) );
                }

                serArray.Add( elementData );

                if( s.ShouldPause() )
                {
                    return SerializationResult.Paused;
                }
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

        public override SerializationResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( data == null )
            {
                return SerializationResult.PrimitiveFinished;
            }

            TSource sourceObj = (obj == null) ? default : (TSource)(object)obj;

            SerializedArray array = (SerializedArray)data["value"];
            int length = array.Count;

            if( populate )
            {
                _objectHasBeenInstantiated = true;
            }
            else
            {
                if( _lateFactory == null && !_objectHasBeenInstantiated )
                {
                    sourceObj = InstantiateEarly( data, l, length );
                    _objectHasBeenInstantiated = true;
                }
                else
                {
                    _factoryElementStorage ??= new TElement[length];
                }
            }

            //
            //
            //

            if( _retryElements != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryElements )
                {
                    SerializedData elementData = array[i];

                    SerializationResult elementResult = entry.mapping.SafeLoad<TElement>( ref entry.value, elementData, l, false );
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = l.CurrentPass;
                    }
                    else if( elementResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    if( elementResult.HasFlag( SerializationResult.Finished ) )
                    {
                        if( _objectHasBeenInstantiated )
                        {
                            elementSetter.Invoke( sourceObj, i, entry.value );
                        }
                        else if( !populate )
                        {
                            _factoryElementStorage[i] = entry.value;
                        }
                    }

                    if( l.ShouldPause() )
                    {
                        foreach( var ii in retryMembersThatSucceededThisTime )
                        {
                            _retryElements.Remove( ii );
                        }
                        obj = (TMember)(object)sourceObj;
                        return SerializationResult.Paused;
                    }
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryElements.Remove( i );
                }
            }

            //
            //
            //

            for( int i = _startIndex; i < length; i++ )
            {
                SerializedData elementData = array[i];

                Type memberType = MappingHelper.GetSerializedType<TElement>( elementData );
                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, memberType );

                TElement elementObj = default;
                SerializationResult elementResult = mapping.SafeLoad<TElement>( ref elementObj, elementData, l, false );
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
                        _retryElements.Add( i, new RetryEntry<TElement>( elementObj, mapping, -1 ) );
                    else
                        _retryElements.Add( i, new RetryEntry<TElement>( elementObj, mapping, l.CurrentPass ) );
                }

                if( elementResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( _objectHasBeenInstantiated )
                    {
                        elementSetter.Invoke( sourceObj, i, elementObj );
                    }
                    else if( !populate )
                    {
                        _factoryElementStorage[i] = elementObj;
                    }
                }

                if( l.ShouldPause() )
                {
                    obj = (TMember)(object)sourceObj;
                    return SerializationResult.Paused;
                }
            }

            SerializationResult result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryElements != null && _retryElements.Count != 0 )
                result |= SerializationResult.HasFailures;
            if( _retryElements == null || _retryElements.Count == 0 )
                result |= SerializationResult.Finished;

            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;

            if( !populate && !_objectHasBeenInstantiated && result.HasFlag( SerializationResult.Finished ) )
            {
                sourceObj = InstantiateLate( data, l );
                _objectHasBeenInstantiated = true;
            }

            obj = (TMember)(object)sourceObj;
            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource InstantiateEarly( SerializedData data, ILoader l, int elemCount )
        {
            // early - raw or earlyfactory or activator
            // late - late factory or activator.
            TSource obj;
            if( _factory != null )
            {
                obj = (TSource)_factory.Invoke( elemCount );
            }
            else if( _rawFactory != null )
            {
                obj = (TSource)_rawFactory.Invoke( data, l );
            }
            else
            {
                if( data == null )
                    return default;

                obj = Activator.CreateInstance<TSource>();
            }

            if( data.TryGetValue( KeyNames.ID, out var id ) )
            {
                l.RefMap.SetObj( id.DeserializeGuid(), obj );
            }

            return obj;
#warning TODO - I think this can still result in a weird infinite-loop-like thing when a skip happens. needs testing.
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource InstantiateLate( SerializedData data, ILoader l )
        {
            // early - raw or earlyfactory or activator
            // late - late factory or activator.
            TSource obj;

            if( _lateFactory != null )
            {
                obj = (TSource)_lateFactory.Invoke( _factoryElementStorage );
            }
            else
            {
                if( data == null )
                    return default;

                obj = Activator.CreateInstance<TSource>();
            }

            if( data.TryGetValue( KeyNames.ID, out var id ) )
            {
                l.RefMap.SetObj( id.DeserializeGuid(), obj );
            }

            return obj;
        }

        public IndexedSerializationMapping<TSource, TElement> WithRawFactory( Func<SerializedData, ILoader, object> rawFactory )
        {
            this._rawFactory = rawFactory;
            return this;
        }

        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<IEnumerable<TElement>, object> factory )
        {
            this._lateFactory = factory;
            // loads elements into a list first, then passes that list here.
            // can be used for e.g. readonlylist.
            return this;
        }

        /// <summary>
        /// Input integer is the element count
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<int, object> factory )
        {
            this._factory = factory;
            return this;
        }
    }
}