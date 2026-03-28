using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that the referenced object couldn't have been resolved during deserialization.
    /// </summary>
    public class UPSMemberResolutionException : UPSSerializationException
    {
        public UPSMemberResolutionException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSMemberResolutionException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
            : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSMemberResolutionException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}