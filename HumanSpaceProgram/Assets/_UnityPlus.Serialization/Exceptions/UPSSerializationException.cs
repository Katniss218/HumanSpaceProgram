using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that an exception occurred during serialization or deserialization.
    /// </summary>
    public class UPSSerializationException : Exception
    {
        /// <summary>
        /// The debug info for the serialization operation when the exception occurred.
        /// </summary>
        public IReadonlySerializationContext Context { get; }
        /// <summary>
        /// Shorthand to get the log from the context, which may contain additional errors and issues.
        /// </summary>
        public SerializationLog Log => Context?.Log;


        public UPSSerializationException( IReadonlySerializationContext ctx )
        {
            Context = ctx;
        }

        public UPSSerializationException( IReadonlySerializationContext ctx, string message )
            : base( message )
        {
            Context = ctx;
        }

        public UPSSerializationException( IReadonlySerializationContext ctx, string message, Exception innerException )
            : base( message, innerException )
        {
            Context = ctx;
        }



        protected UPSSerializationException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}