using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A type of mapping that operates on a primitive type.
    /// </summary>
    /// <remarks>
    /// A primitive type is one that can't be paused mid-save/load. Generally should be small, of fixed size, and not generic.
    /// </remarks>
    /// <typeparam name="TSource">The type being mapped.</typeparam>
    public sealed class PrimitiveSerializationMapping<TSource> : SerializationMapping
    {
        /// <summary>
        /// The function invoked to convert the C# object into its serialized representation.
        /// </summary>
        public Func<TSource, ISaver, SerializedData> OnSave { get; set; }

        /// <summary>
        /// The function invoked to convert the serialized representation back into its corresponding C# object.
        /// </summary>
        public Func<SerializedData, IForwardReferenceMap, TSource> OnLoad { get; set; }

        /// <summary>
        /// Force the serializer to skip the object header and serialize inline.
        /// </summary>
        bool _skipHeader;

        public PrimitiveSerializationMapping()
        {

        }

        public PrimitiveSerializationMapping( bool skipHeader )
        {
            _skipHeader = skipHeader;
        }

        public override SerializationMapping GetInstance()
        {
            return new PrimitiveSerializationMapping<TSource>()
            {
                Context = this.Context,
                OnSave = this.OnSave, // copy since it stores the member obj var.
                OnLoad = this.OnLoad,
                _skipHeader = this._skipHeader
            };
        }

        public override SerializationResult Save<TMember>( TMember obj, ref SerializedData data, ISaver s )
        {
            if( OnSave == null )
                return SerializationResult.PrimitiveFinishedFailed;

            if( !_skipHeader && obj != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() ) // This doesn't appear to slow the system down much at all when benchbarked.
            {
                data = new SerializedObject();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();        // DOES make sense for structs (they may be boxed in an `object` or an interface)
                data[KeyNames.ID] = s.RefMap.GetID( obj ).SerializeGuid(); // doesn't make sense for structs, even if boxed.

                try
                {
                    data["value"] = OnSave.Invoke( (TSource)(object)obj, s );
                }
                catch
                {
                    return SerializationResult.PrimitiveRetryFailed;
                }
            }
            else
            {
                try
                {
                    data = OnSave.Invoke( (TSource)(object)obj, s );
                }
                catch
                {
                    return SerializationResult.PrimitiveRetryFailed;
                }
            }

            return SerializationResult.PrimitiveFinished;
        }

        public override SerializationResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate )
        {
            if( OnLoad == null )
                return SerializationResult.PrimitiveFinishedFailed;

            if( !_skipHeader && data != null && MappingHelper.IsNonNullEligibleForTypeHeader<TMember>() ) // This doesn't appear to slow the system down much at all when benchbarked.
            {
                try
                {
                    TSource obj2 = OnLoad.Invoke( data["value"], l.RefMap );
                    obj = (TMember)(object)obj2;
                }
                catch
                {
                    return SerializationResult.PrimitiveRetryFailed;
                }
            }
            else
            {
                try
                {
                    TSource obj2 = OnLoad.Invoke( data, l.RefMap );
                    obj = (TMember)(object)obj2;
                }
                catch
                {
                    return SerializationResult.PrimitiveRetryFailed;
                }
            }
            return SerializationResult.PrimitiveFinished;
        }
    }
}