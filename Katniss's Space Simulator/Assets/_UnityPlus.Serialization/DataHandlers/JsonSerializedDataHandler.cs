using System;
using System.IO;
using System.Text;
using UnityPlus.Serialization.Json;

namespace UnityPlus.Serialization.DataHandlers
{
    /// <summary>
    /// Reads and writes serialized data from a json file on disk.
    /// </summary>
    public class JsonSerializedDataHandler : ISerializedDataHandler
    {
        /// <summary>
        /// The path to the json file.
        /// </summary>
        public string Filepath { get; set; }

        public JsonSerializedDataHandler( string filepath )
        {
            this.Filepath = filepath;
        }

        public SerializedData Read()
        {
            string fileContents = File.ReadAllText( Filepath, Encoding.UTF8 );

            SerializedData s = new JsonStringReader( fileContents ).Read();

            return s;
        }

        public void Write( SerializedData data )
        {
            StringBuilder stringBuilder = new StringBuilder();

            new JsonStringWriter( data, stringBuilder ).Write();

            string dirPath = Path.GetDirectoryName( Filepath );

            if( !Directory.Exists( dirPath ) )
            {
                Directory.CreateDirectory( dirPath );
            }
            File.WriteAllText( Filepath, stringBuilder.ToString(), Encoding.UTF8 );
        }
    }
}