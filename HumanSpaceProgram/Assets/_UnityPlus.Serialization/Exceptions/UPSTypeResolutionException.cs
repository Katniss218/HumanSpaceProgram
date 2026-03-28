using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that a type could not be resolved during deserialization.
    /// </summary>
    public class UPSTypeResolutionException : UPSSerializationException
    {
        public UPSTypeResolutionException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSTypeResolutionException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
            : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSTypeResolutionException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}