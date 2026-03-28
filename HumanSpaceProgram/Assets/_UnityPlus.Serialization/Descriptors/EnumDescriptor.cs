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
}

namespace UnityPlus.Serialization.Descriptors
{
    public class EnumDescriptor<T> : PrimitiveDescriptor<T> where T : struct, Enum
    {
        private readonly EnumSerializationMode _mode;

        public EnumDescriptor() : this( EnumSerializationMode.String )
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
            if( data is not SerializedPrimitive prim )
                return DeserializationResult.Failed;

            if( _mode == EnumSerializationMode.String )
            {
                if( prim.TryGetString( out string val ) && Enum.TryParse<T>( val, true, out var res ) )
                {
                    result = res;
                    return DeserializationResult.Success;
                }
                ctx.Log.Log( LogLevel.Warning, $"Failed to parse enum value '{prim}' for type '{typeof( T ).FullName}'." );
            }
            else
            {
                result = Enum.ToObject( typeof( T ), (long)prim );
                return DeserializationResult.Success;
            }

            return DeserializationResult.Failed;
        }
    }
}