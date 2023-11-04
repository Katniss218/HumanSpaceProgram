using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.Json;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A serialized data handler that reads and writes objects and data to and from a common json file.
    /// </summary>
    public class JsonSingleFileSerializedDataHandler : ISerializedDataHandler
    {
        /// <summary>
        /// The file with object instances themselves.
        /// </summary>
        public string Filename { get; set; }

        const string OBJECTS_KEY = "objects";
        const string DATA_KEY = "data";

        public (SerializedData o, SerializedData d) ReadObjectsAndData()
        {
            string oContents = File.ReadAllText( Filename, Encoding.UTF8 );

            SerializedData s = new JsonStringReader( oContents ).Read();

            return (s[OBJECTS_KEY], s[DATA_KEY]);
        }

        public void WriteObjectsAndData( SerializedData o, SerializedData d )
        {
            StringBuilder contents = new StringBuilder();

            var s = new SerializedObject()
            {
                { OBJECTS_KEY, o },
                { DATA_KEY, d }
            };

            new JsonStringWriter( o, contents ).Write();

            Directory.CreateDirectory( Path.GetDirectoryName( Filename ) );
            File.WriteAllText( Filename, contents.ToString(), Encoding.UTF8 );
        }
    }
}