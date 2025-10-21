using System;
using System.Runtime.Serialization;

namespace HSP.Content.Mods
{
    /// <summary>
    /// Exception thrown when a specific mod fails to load.
    /// </summary>
    public class ModLoadException : ModLoaderException
    {
        /// <summary>
        /// The ID of the mod that failed to load.
        /// </summary>
        public string ModID { get; }

        public ModLoadException( string modId ) : base( $"Failed to load mod '{modId}'." )
        {
            ModID = modId;
        }

        public ModLoadException( string modId, string message ) : base( message )
        {
            ModID = modId;
        }

        public ModLoadException( string modId, Exception inner ) : base( $"Failed to load mod '{modId}': {inner?.Message}", inner )
        {
            ModID = modId;
        }

        public ModLoadException( string modId, string message, Exception inner ) : base( message, inner )
        {
            ModID = modId;
        }

        // Serialization constructor
        protected ModLoadException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}