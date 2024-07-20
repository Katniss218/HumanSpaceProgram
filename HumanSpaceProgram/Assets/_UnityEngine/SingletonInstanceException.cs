using System;
using System.Runtime.Serialization;

namespace UnityEngine
{
    public class SingletonInstanceException : Exception
    {
        public SingletonInstanceException()
        {
        }

        public SingletonInstanceException( string message ) : base( message )
        {
        }

        public SingletonInstanceException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected SingletonInstanceException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}