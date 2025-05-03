using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Like primitive, but can be paused/retried.
    /// </summary>
    /// <typeparam name="TSource">The type wrapped inside the nullable.</typeparam>
    public sealed class NullableSerializationMapping<TSource> : SerializationMapping where TSource : struct
    {
        RetryEntry<TSource> _retryMember;

        bool _wasFailureNoRetry = false;

        public NullableSerializationMapping()
        {
        }

        private NullableSerializationMapping( NullableSerializationMapping<TSource> copy )
        {
            this.Context = copy.Context;
        }

        public override SerializationMapping GetInstance()
        {
            return new NullableSerializationMapping<TSource>( this );
        }

        //
        //  Mapping methods:
        //

        public override SerializationResult Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return SerializationResult.Finished;
            }

            Nullable<TSource> sourceObj = (Nullable<TSource>)(object)obj;

            //
            //      RETRY PREVIOUSLY FAILED MEMBERS
            //
            if( _retryMember != null )
            {
                if( _retryMember.pass != s.CurrentPass )
                {
                    var memberResult = _retryMember.mapping.SafeSave<TSource>( (TSource)_retryMember.value, ref data, s );
                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                    {
                        _retryMember.pass = s.CurrentPass;
                    }
                    else if( memberResult.HasFlag( SerializationResult.Finished ) )
                    {
                        _retryMember = null;
                    }
                }
            }

            //
            //      PROCESS THE MEMBERS THAT HAVE NOT FAILED YET.
            //
            else
            {
                TSource memberObj = sourceObj.Value;
                var mapping = SerializationMappingRegistry.GetMapping<TSource>( Context, memberObj );
                var memberResult = mapping.SafeSave<TSource>( memberObj, ref data, s );

                if( memberResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;
                }
                else
                {
                    _retryMember = memberResult.HasFlag( SerializationResult.Paused )
                        ? new RetryEntry<TSource>( memberObj, mapping, -1 )
                        : new RetryEntry<TSource>( memberObj, mapping, s.CurrentPass );
                }
            }

            return ComputeSerializationResult();
        }

        public override SerializationResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( data == null )
            {
                return SerializationResult.Finished;
            }

            Nullable<TSource> sourceObj = (obj == null) ? null : (Nullable<TSource>)(object)obj;

            //
            //      RETRY PREVIOUSLY FAILED MEMBERS
            //
            if( _retryMember != null )
            {
                if( _retryMember.pass == l.CurrentPass )
                {
                    TSource memberObj = (TSource)_retryMember.value; // null/default or nullable.Value
                    var memberResult = _retryMember.mapping.SafeLoad<TSource>( ref memberObj, data, l, false );

                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                    {
                        _retryMember.pass = l.CurrentPass;
                        _retryMember.value = memberObj;
                    }
                    else if( memberResult.HasFlag( SerializationResult.Finished ) )
                    {
                        sourceObj = (Nullable<TSource>)memberObj; // assign when finished.
                        _retryMember = null;
                    }
                }
            }

            //
            //      PROCESS THE MEMBERS THAT HAVE NOT FAILED YET.
            //
            else
            {
                Type memberType = MappingHelper.GetSerializedType<TSource>( data );
                var mapping = SerializationMappingRegistry.GetMapping<TSource>( Context, memberType );

                TSource memberObj = default;
                SerializationResult memberResult = mapping.SafeLoad<TSource>( ref memberObj, data, l, false );

                if( memberResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( memberResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;
                    else
                        sourceObj = (Nullable<TSource>)memberObj;
                }
                else
                {
                    _retryMember = memberResult.HasFlag( SerializationResult.Paused )
                        ? new RetryEntry<TSource>( memberObj, mapping, -1 )
                        : new RetryEntry<TSource>( memberObj, mapping, l.CurrentPass );
                }
            }

            obj = (TMember)(object)sourceObj;
            return ComputeSerializationResult();
        }

        private SerializationResult ComputeSerializationResult()
        {
            var result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryMember != null )
                result |= SerializationResult.HasFailures;
            if( _retryMember == null )
                result |= SerializationResult.Finished;
            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;
            return result;
        }
    }
}