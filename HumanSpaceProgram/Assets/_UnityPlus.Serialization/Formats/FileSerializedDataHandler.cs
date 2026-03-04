using System;
using System.IO;

namespace UnityPlus.Serialization.Formats
{
    /// <summary>
    /// Reads and writes serialized data to a file path using a provided Format.
    /// Handles directory creation and file stream management.
    /// </summary>
    public class FileSerializedDataHandler : ISerializedDataHandler
    {
        public string Filepath { get; set; }
        public ISerializationFormat Format { get; }

        public FileSerializedDataHandler( string filepath, ISerializationFormat format )
        {
            if( string.IsNullOrEmpty( filepath ) ) throw new ArgumentNullException( nameof( filepath ) );
            if( format == null ) throw new ArgumentNullException( nameof( format ) );

            this.Filepath = filepath;
            this.Format = format;
        }

        public SerializedData Read()
        {
            if( !File.Exists( Filepath ) )
                return null;

            using( FileStream fs = File.OpenRead( Filepath ) )
            {
                return Format.Read( fs );
            }
        }

        public void Write( SerializedData data )
        {
            string dirPath = Path.GetDirectoryName( Filepath );
            if( !string.IsNullOrEmpty( dirPath ) && !Directory.Exists( dirPath ) )
            {
                Directory.CreateDirectory( dirPath );
            }

            using( FileStream fs = File.Create( Filepath ) )
            {
                Format.Write( fs, data );
            }
        }
    }
}