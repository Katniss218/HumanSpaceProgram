using System;
using System.Runtime.Serialization;

namespace HSP.Timelines
{
    public class SaveMigrationException : Exception
    {
        public SaveMigrationException() : base( $"Failed to migrate a save." )
        {
        }

        public SaveMigrationException( string message ) : base( message )
        {
        }

        public SaveMigrationException( Exception inner ) : base( $"Failed to migrate a save: {inner?.Message}", inner )
        {
        }

        public SaveMigrationException( string message, Exception inner ) : base( message, inner )
        {
        }

        // Serialization constructor
        protected SaveMigrationException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}