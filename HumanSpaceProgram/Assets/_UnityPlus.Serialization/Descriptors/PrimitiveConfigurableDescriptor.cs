
using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A primitive descriptor that delegates logic to functions.
    /// Useful for defining serialization logic inline within factory methods.
    /// </summary>
    public class PrimitiveConfigurableDescriptor<T> : PrimitiveDescriptor<T>
    {
        private readonly Action<T, SerializedDataWrapper, SerializationContext> _serializer;
        private readonly Func<SerializedData, SerializationContext, T> _deserializer;

        /// <summary>
        /// Wrapper to allow setting the ref parameter inside the lambda.
        /// </summary>
        public class SerializedDataWrapper
        {
            public SerializedData Data;
        }

        public PrimitiveConfigurableDescriptor( Action<T, SerializedDataWrapper, SerializationContext> serializer, Func<SerializedData, SerializationContext, T> deserializer )
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var wrapper = new SerializedDataWrapper { Data = data };
            _serializer( (T)target, wrapper, ctx );
            data = wrapper.Data;
        }

        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            try
            {
                result = _deserializer( data, ctx );
                return DeserializationResult.Success;
            }
            catch
            {
                result = default( T );
                return DeserializationResult.Failed;
            }
        }
    }
}