using System;
using System.Runtime.Serialization;

namespace HSP.Content.Mods
{
    /// <summary>
    /// A base class for exceptions related to mod loading.
    /// </summary>
    public class ModLoaderException : Exception
    {
        public ModLoaderException() { }
        public ModLoaderException( string message ) : base( message ) { }
        public ModLoaderException( string message, Exception inner ) : base( message, inner ) { }

        // Serialization constructor
        protected ModLoaderException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}