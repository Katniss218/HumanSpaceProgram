using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.Json;

namespace UnityPlus.Serialization.DataHandlers
{
    /// <summary>
    /// A serialized data handler that reads and writes objects and data to and from a common json file.
    /// </summary>
    public class JsonSerializedDataHandler : ISerializedDataHandler
    {
        public string Filename { get; set; }

        public SerializedData Read()
        {
            string oContents = File.ReadAllText( Filename, Encoding.UTF8 );

            SerializedData s = new JsonStringReader( oContents ).Read();

            return s;
        }

        public void Write( SerializedData data )
        {
            StringBuilder stringBuilder = new StringBuilder();

            new JsonStringWriter( data, stringBuilder ).Write();

            string dirPath = Path.GetDirectoryName( Filename );

            if( !Directory.Exists( dirPath ) )
            {
                Directory.CreateDirectory( dirPath );
            }
            File.WriteAllText( Filename, stringBuilder.ToString(), Encoding.UTF8 );
        }
    }
}