using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that the serialized data does not match the expected structure or type.
    /// </summary>
    public class UPSDataMismatchException : UPSSerializationException
    {
        public UPSDataMismatchException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSDataMismatchException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
            : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSDataMismatchException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}