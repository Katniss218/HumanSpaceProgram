using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Represents additional metadata of a saved vessel.
    /// </summary>
    public class VesselMetadata
    {
        /// <summary>
        /// The name of the file that stores the vessel metadata.
        /// </summary>
        public const string VESSEL_FILENAME = "_vessel.json";

        /// <summary>
        /// The unique ID of this part.
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        public VesselMetadata( string id )
        {
            this.ID = id;
        }

        /// <summary>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </summary>
        public static string GetSavedVesselsPath()
        {
            return HumanSpaceProgram.GetSavedVesselsDirectoryPath();
        }

        /// <summary>
        /// Root directory is the directory that contains the _vessel.json file.
        /// </summary>
        public static string GetRootDirectory( string vesselId )
        {
            return Path.Combine( GetSavedVesselsPath(), vesselId );
        }

        /// <summary>
        /// Returns the path to the (root) directory of the timeline.
        /// </summary>
        public string GetRootDirectory()
        {
            return GetRootDirectory( this.ID );
        }

        public void WriteToDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, VESSEL_FILENAME );

            StringBuilder sb = new StringBuilder();
            new JsonStringWriter( this.GetData(), sb ).Write();

            File.WriteAllText( saveFilePath, sb.ToString(), Encoding.UTF8 );
        }

        public void ReadDataFromDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, VESSEL_FILENAME );

            string saveJson = File.ReadAllText( saveFilePath, Encoding.UTF8 );

            SerializedData data = new JsonStringReader( saveJson ).Read();

            this.SetData( data );
        }

        public SerializedData GetData()
        {
            return new SerializedObject()
            {
                { "name", this.Name },
                { "description", this.Description },
            };
        }

        public void SetData( SerializedData data )
        {
            if( data.TryGetValue( "name", out var name ) )
            {
                this.Name = (string)name;
            }
            if( data.TryGetValue( "description", out var description ) )
            {
                this.Description = (string)description;
            }
        }
    }
}