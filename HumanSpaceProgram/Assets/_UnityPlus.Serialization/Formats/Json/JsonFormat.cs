using System.IO;
using UnityPlus.Serialization.Json;

namespace UnityPlus.Serialization.Formats
{
    /// <summary>
    /// UTF-8 JSON format implementation.
    /// </summary>
    public class JsonFormat : ISerializationFormat
    {
        public static readonly JsonFormat Instance = new JsonFormat();

        public SerializedData Read( Stream stream )
        {
            var reader = new JsonStreamReader( stream );
            return reader.Read();
        }

        public void Write( Stream stream, SerializedData data )
        {
            var writer = new JsonStreamWriter( data, stream );
            writer.Write();
        }
    }
}
