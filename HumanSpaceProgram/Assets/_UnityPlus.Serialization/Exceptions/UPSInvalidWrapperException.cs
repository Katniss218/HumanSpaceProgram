using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    public class UPSInvalidWrapperException : UPSDataMismatchException
    {
        public UPSInvalidWrapperException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSInvalidWrapperException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
           : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSInvalidWrapperException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}