
using System;
using System.IO;

namespace UnityPlus.Serialization.Json
{
    /// <summary>
    /// A wrapper around JsonStreamReader that reads from a string.
    /// </summary>
    public class JsonStringReader
    {
        private readonly string _json;

        public JsonStringReader( string json )
        {
            _json = json;
        }

        public SerializedData Read()
        {
            if( string.IsNullOrEmpty( _json ) )
                return null;

            // Wrap the string in a StringReader (TextReader) and delegate to JsonStreamReader.
            // While this adds a small allocation for StringReader, it eliminates 300+ lines of duplicate parsing logic.
            using( var reader = new StringReader( _json ) )
            {
                var streamReader = new JsonStreamReader( reader );
                return streamReader.Read();
            }
        }
    }
}
