using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that the referenced object was not instantiated (yet) during deserialization.
    /// </summary>
    public class UPSMissingReferenceException : UPSSerializationException
    {
        public UPSMissingReferenceException()
        {
        }

        public UPSMissingReferenceException( string message )
            : base( message )
        {
        }

        public UPSMissingReferenceException( string message, Exception innerException )
            : base( message, innerException )
        {
        }



        protected UPSMissingReferenceException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
