using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that a deadlock occurred during deferred operation resolution.
    /// </summary>
    public class UPSDeferredDeadlockException : UPSDeferredOperationException
    {
        public UPSDeferredDeadlockException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSDeferredDeadlockException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
            : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSDeferredDeadlockException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}