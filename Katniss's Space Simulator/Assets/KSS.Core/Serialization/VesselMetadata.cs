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
        public const string VESSEL_METADATA_FILENAME = "_vessel.json";

        /// <summary>
        /// The unique ID of this vessel.
        /// </summary>
        public readonly string ID; // Vessels don't have a namespace, they're player-created.

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The name of the author of the vessel.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The version of the vessel file.
        /// </summary>
        public Version FileVersion { get; set; }

        /// <summary>
        /// The versions of all the mods used when the vessel file was created.
        /// </summary>
        public Dictionary<string, Version> ModVersions { get; set; } = new Dictionary<string, Version>();

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
            string saveFilePath = Path.Combine( savePath, VESSEL_METADATA_FILENAME );

            StringBuilder sb = new StringBuilder();
            new JsonStringWriter( this.GetData(), sb ).Write();

            File.WriteAllText( saveFilePath, sb.ToString(), Encoding.UTF8 );
        }

        public void ReadDataFromDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, VESSEL_METADATA_FILENAME );

            string saveJson = File.ReadAllText( saveFilePath, Encoding.UTF8 );

            SerializedData data = new JsonStringReader( saveJson ).Read();

            this.SetData( data );
        }

        public SerializedData GetData()
        {
            SerializedObject modVersions = new SerializedObject();
            foreach( var elemKvp in this.ModVersions )
            {
                modVersions.Add( elemKvp.Key, elemKvp.Value.ToString() );
            }
            return new SerializedObject()
            {
                { "name", this.Name },
                { "description", this.Description },
                { "author", this.Author },
                { "file_version", this.FileVersion.ToString() },
                { "mod_versions", modVersions }
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
            if( data.TryGetValue( "author", out var author ) )
            {
                this.Author = (string)author;
            }
            if( data.TryGetValue( "file_version", out var saveVersion ) )
            {
                this.FileVersion = Version.Parse( (string)saveVersion );
            }

            if( data.TryGetValue( "mod_versions", out var modVersions ) )
            {
                this.ModVersions = new Dictionary<string, Version>();
                foreach( var elemKvp in (SerializedObject)modVersions )
                {
                    this.ModVersions.Add( elemKvp.Key, Version.Parse( (string)elemKvp.Value ) );
                }
            }
        }
    }
}