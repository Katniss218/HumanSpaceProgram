using System;
using System.Runtime.Serialization;

namespace HSP.Content.Migrations
{
    public class MigrationException : Exception
    {
        public MigrationException() : base( $"Failed to migrate." )
        {
        }

        public MigrationException( string message ) : base( message )
        {
        }

        public MigrationException( Exception inner ) : base( $"Failed to migrate: {inner?.Message}", inner )
        {
        }

        public MigrationException( string message, Exception inner ) : base( message, inner )
        {
        }

        // Serialization constructor
        protected MigrationException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}