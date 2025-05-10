using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    public class UPSSerializationException : Exception
    {
        public UPSSerializationException()
        {
        }

        public UPSSerializationException( string message )
            : base( message )
        {
        }

        public UPSSerializationException( string message, Exception innerException )
            : base( message, innerException )
        {
        }



        protected UPSSerializationException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}