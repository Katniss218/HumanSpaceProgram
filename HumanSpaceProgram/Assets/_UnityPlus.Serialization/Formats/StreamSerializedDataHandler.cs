using System;
using System.IO;

namespace UnityPlus.Serialization.Formats
{
    /// <summary>
    /// Reads and writes serialized data to a specific Stream using a provided Format.
    /// Does not close the stream automatically.
    /// </summary>
    public class StreamSerializedDataHandler : ISerializedDataHandler
    {
        public Stream Stream { get; }
        public ISerializationFormat Format { get; }

        public StreamSerializedDataHandler( Stream stream, ISerializationFormat format )
        {
            if( stream == null ) throw new ArgumentNullException( nameof( stream ) );
            if( format == null ) throw new ArgumentNullException( nameof( format ) );

            this.Stream = stream;
            this.Format = format;
        }

        public SerializedData Read()
        {
            if( !Stream.CanRead )
                throw new InvalidOperationException( "Stream is not readable." );

            return Format.Read( Stream );
        }

        public void Write( SerializedData data )
        {
            if( !Stream.CanWrite )
                throw new InvalidOperationException( "Stream is not writable." );

            Format.Write( Stream, data );
        }
    }
}