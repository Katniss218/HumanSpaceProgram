using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A type of mapping that operates on a collection type that is not indexable.
    /// </summary>
    /// <remarks>
    /// Failed elements will be appended after elements that have succeeded. <br/>
    /// Assumes that the collection type <typeparamref name="TSource"/> returns its elements in a consistent order.
    /// </remarks>
    /// <typeparam name="TSource">The type being mapped.</typeparam>
    public class EnumeratedSerializationMapping<TSource, TElement> : SerializationMapping where TSource : IEnumerable<TElement>
    {
        int elementContext;
        Action<TSource, int, TElement> elementSetter;
        private bool _objectHasBeenInstantiated;

        TElement[] _factoryElementStorage;
        int _startIndex = 0;
        Dictionary<int, RetryEntry<TElement>> _retryElements;

        Func<SerializedData, ILoader, object> _rawFactory = null;
        Func<int, object> factory;
        Func<IEnumerable<TElement>, object> lateFactory;

        public EnumeratedSerializationMapping( Action<TSource, int, TElement> setter )
        {
            elementContext = 0;
            this.elementSetter = setter;
        }

        public EnumeratedSerializationMapping( int elementContext, Action<TSource, int, TElement> setter )
        {
            this.elementContext = elementContext;
            this.elementSetter = setter;
        }

        public override SerializationMapping GetInstance()
        {
            return new EnumeratedSerializationMapping<TSource, TElement>( elementContext, elementSetter )
            {
                Context = Context,
            };
        }

        public override MappingResult Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return MappingResult.Finished;
            }

            TSource sourceObj = (TSource)(object)obj;

            SerializedArray serArray;
            if( data == null )
            {
                data = new SerializedObject();
                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();

                serArray = new SerializedArray();
                data["value"] = serArray;
            }
            else
            {
                serArray = (SerializedArray)data["value"];
            }

            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            //
            //
            //

            if( _retryElements != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryElements )
                {
                    SerializedData elementData = null;

                    MappingResult elementResult = entry.mapping.SafeSave<TElement>( entry.value, ref elementData, s );
                    switch( elementResult )
                    {
                        case MappingResult.Finished:
                            retryMembersThatSucceededThisTime.Add( i );
                            anyFinished = true;
                            break;
                        case MappingResult.Failed:
                            anyFailed = true;
                            break;
                        case MappingResult.Progressed:
                            anyProgressed = true;
                            break;
                    }

                    serArray[i] = elementData;

                    if( s.ShouldPause() )
                    {
                        break;
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

            int index = 0;
            foreach( TElement elementObj in sourceObj )
            {
                if( index < _startIndex ) // Assumes that the enumerable enumerates in the same order each time.
                {
                    index++;
                    continue;
                }

                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, elementObj );

                SerializedData elementData = null;

                MappingResult elementResult = mapping.SafeSave( elementObj, ref elementData, s );
                switch( elementResult )
                {
                    case MappingResult.Finished:
                        _startIndex = index + 1;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        _retryElements ??= new();
                        _retryElements.Add( index, new RetryEntry<TElement>( elementObj, mapping ) );
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        _retryElements ??= new();
                        _retryElements.Add( index, new RetryEntry<TElement>( elementObj, mapping ) );
                        anyProgressed = true;
                        break;
                }

                serArray.Add( elementData );
                index++;

                if( s.ShouldPause() )
                {
                    break;
                }
            }

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }

        public override MappingResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( data == null )
            {
                return MappingResult.Finished;
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
                if( lateFactory == null && !_objectHasBeenInstantiated )
                {
                    sourceObj = InstantiateEarly( data, l, length );
                    _objectHasBeenInstantiated = true;
                }
                else
                {
                    _factoryElementStorage ??= new TElement[length];
                }
            }

            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            //
            //
            //

            if( _retryElements != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryElements )
                {
                    SerializedData elementData = array[i];

                    MappingResult elementResult = entry.mapping.SafeLoad<TElement>( ref entry.value, elementData, l, false );
                    switch( elementResult )
                    {
                        case MappingResult.Finished:
                            retryMembersThatSucceededThisTime.Add( i );
                            anyFinished = true;
                            break;
                        case MappingResult.Failed:
                            anyFailed = true;
                            break;
                        case MappingResult.Progressed:
                            anyProgressed = true;
                            break;
                    }

                    if( elementResult == MappingResult.Finished )
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
                        break;
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
                MappingResult elementResult = mapping.SafeLoad<TElement>( ref elementObj, elementData, l, false );
                switch( elementResult )
                {
                    case MappingResult.Finished:
                        _startIndex = i + 1;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        _retryElements ??= new();
                        _retryElements.Add( i, new RetryEntry<TElement>( elementObj, mapping ) );
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        _retryElements ??= new();
                        _retryElements.Add( i, new RetryEntry<TElement>( elementObj, mapping ) );
                        anyProgressed = true;
                        break;
                }

                if( elementResult == MappingResult.Finished )
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
                    break;
                }
            }

            if( !populate && !_objectHasBeenInstantiated )
            {
                sourceObj = InstantiateLate( data, l );
                _objectHasBeenInstantiated = true;
            }

            obj = (TMember)(object)sourceObj;
            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource InstantiateEarly( SerializedData data, ILoader l, int elemCount )
        {
            // early - raw or earlyfactory or activator
            // late - late factory or activator.
            TSource obj;
            if( factory != null )
            {
                obj = (TSource)factory.Invoke( elemCount );
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
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource InstantiateLate( SerializedData data, ILoader l )
        {
            // early - raw or earlyfactory or activator
            // late - late factory or activator.
            TSource obj;

            if( lateFactory != null )
            {
                obj = (TSource)lateFactory.Invoke( _factoryElementStorage );
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

        public EnumeratedSerializationMapping<TSource, TElement> WithRawFactory( Func<SerializedData, ILoader, object> rawFactory )
        {
            this._rawFactory = rawFactory;
            return this;
        }

        public EnumeratedSerializationMapping<TSource, TElement> WithFactory( Func<IEnumerable<TElement>, object> factory )
        {
            this.lateFactory = factory;
            // loads elements into a list first, then passes that list here.
            // can be used for e.g. readonlylist.
            return this;
        }

        /// <summary>
        /// Input integer is the element count
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public EnumeratedSerializationMapping<TSource, TElement> WithFactory( Func<int, object> factory )
        {
            this.factory = factory;
            return this;
        }
    }
}