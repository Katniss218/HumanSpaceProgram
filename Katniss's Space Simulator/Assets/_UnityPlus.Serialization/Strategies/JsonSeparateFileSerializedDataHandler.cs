using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.Json;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// A serialized data handler that reads and writes objects and data to and from 2 separate json files.
    /// </summary>
    public class JsonSeparateFileSerializedDataHandler : ISerializedDataHandler
    {
        /// <summary>
        /// The file with object instances themselves.
        /// </summary>
        public string ObjectsFilename { get; set; }

        /// <summary>
        /// The file with object instance data.
        /// </summary>
        public string DataFilename { get; set; }

        public (SerializedData o, SerializedData d) ReadObjectsAndData()
        {
            string oContents = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            string dContents = File.ReadAllText( DataFilename, Encoding.UTF8 );

            SerializedData o = new JsonStringReader( oContents ).Read();
            SerializedData d = new JsonStringReader( dContents ).Read();

            return (o, d);
        }

        public void WriteObjectsAndData( SerializedData o, SerializedData d )
        {
            StringBuilder oContents = new StringBuilder();
            StringBuilder dContents = new StringBuilder();

            new JsonStringWriter( o, oContents ).Write();
            new JsonStringWriter( d, dContents ).Write();

            Directory.CreateDirectory( Path.GetDirectoryName( ObjectsFilename ) );
            Directory.CreateDirectory( Path.GetDirectoryName( DataFilename ) );
            File.WriteAllText( ObjectsFilename, oContents.ToString(), Encoding.UTF8 );
            File.WriteAllText( DataFilename, dContents.ToString(), Encoding.UTF8 );
        }
    }
}
