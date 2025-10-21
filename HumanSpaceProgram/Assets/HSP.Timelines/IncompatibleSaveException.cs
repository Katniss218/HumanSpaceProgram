using System;
using System.Runtime.Serialization;

namespace HSP.Timelines
{
    public class IncompatibleSaveException : Exception
    {
        public IncompatibleSaveException() : base( $"The save nad mods that are missing from the game." )
        {
        }

        public IncompatibleSaveException( string message ) : base( message )
        {
        }

        public IncompatibleSaveException( Exception innerException ) : base( $"The save nad mods that are missing from the game.", innerException )
        {
        }

        public IncompatibleSaveException( string message, Exception innerException ) : base( message, innerException )
        {
        }



        protected IncompatibleSaveException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}