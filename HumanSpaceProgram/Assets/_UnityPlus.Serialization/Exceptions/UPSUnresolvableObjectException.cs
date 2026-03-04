using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that the referenced object couldn't have been resolved during deserialization.
    /// </summary>
    public class UPSUnresolvableObjectException : UPSSerializationException
    {
        public UPSUnresolvableObjectException( IReadonlySerializationContext ctx )
            : base( ctx )
        {
        }

        public UPSUnresolvableObjectException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }

        public UPSUnresolvableObjectException( IReadonlySerializationContext ctx, string message, Exception innerException )
            : base( ctx, message, innerException )
        {
        }



        protected UPSUnresolvableObjectException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}