using System;

namespace UnityPlus.Serialization
{
    public enum EnumSerializationMode
    {
        /// <summary>
        /// Serializes the enum as its underlying integer value (e.g. 0, 1, 2).
        /// </summary>
        Integer,
        /// <summary>
        /// Serializes the enum as its string name (e.g. "FirstValue", "SecondValue").
        /// </summary>
        String
    }

    public class EnumDescriptor<T> : PrimitiveDescriptor<T> where T : struct, Enum
    {
        private readonly EnumSerializationMode _mode;

        public EnumDescriptor() : this( EnumSerializationMode.Integer )
        {
        }

        public EnumDescriptor( EnumSerializationMode mode )
        {
            _mode = mode;
        }

        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            if( _mode == EnumSerializationMode.String )
            {
                data = (SerializedPrimitive)target.ToString();
            }
            else
            {
                // Box to int/byte/etc then to primitive
                data = (SerializedPrimitive)Convert.ToInt64( target );
            }
        }

        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( T );
            if( data is SerializedPrimitive prim )
            {
                // Reading String
                if( prim._type == SerializedPrimitive.DataType.String )
                {
                    if( Enum.TryParse<T>( (string)prim, true, out var res ) )
                    {
                        result = res;
                        return DeserializationResult.Success;
                    }
                }
                // Reading Number
                else if( prim._type == SerializedPrimitive.DataType.Int64 )
                {
                    result = Enum.ToObject( typeof( T ), (long)prim );
                    return DeserializationResult.Success;
                }
            }
            return DeserializationResult.Failed;
        }
    }
}