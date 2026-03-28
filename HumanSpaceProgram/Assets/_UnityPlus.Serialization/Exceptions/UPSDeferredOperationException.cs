using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that a deferred operation failed to complete.
    /// </summary>
    public class UPSDeferredOperationException : UPSMemberResolutionException
    {
        public UPSDeferredOperationException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSDeferredOperationException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
            : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSDeferredOperationException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}