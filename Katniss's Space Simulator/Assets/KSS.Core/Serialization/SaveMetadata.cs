using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Serializable (meta)data of a specific timeline's save.
    /// </summary>
    public sealed class SaveMetadata
    {
        /// <summary>
        /// The current version of new save files.
        /// </summary>
        public static readonly Version CURRENT_SAVE_FILE_VERSION = new Version( 0, 0 );

        /// <summary>
        /// The name of the file that stores the save metadata.
        /// </summary>
        public const string SAVE_FILENAME = "_save.json";

        /// <summary>
        /// The persistent save's ID. A persistent save is the default save when a custom save is not specified.
        /// </summary>
        public const string PERSISTENT_SAVE_ID = "___persistent";

        /// <summary>
        /// The unique ID of this specific save's timeline.
        /// </summary>
        public readonly string TimelineID;
        /// <summary>
        /// The unique ID of this specific save.
        /// </summary>
        public readonly string SaveID;

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The version of the save file.
        /// </summary>
        public Version FileVersion { get; set; }

        /// <summary>
        /// The versions of all the mods used when the save file was created.
        /// </summary>
        public Dictionary<string, Version> ModVersions { get; set; } = new Dictionary<string, Version>();

        public SaveMetadata( string timelineId, string saveId )
        {
            this.TimelineID = timelineId;
            this.SaveID = saveId;
        }

        /// <summary>
        /// Root directory is the directory that contains the _save.json file.
        /// </summary>
        public string GetRootDirectory()
        {
            return GetRootDirectory( this.TimelineID, this.SaveID );
        }

        /// <summary>
        /// Root directory is the directory that contains the _save.json file.
        /// </summary>
        public static string GetRootDirectory( string timelineId, string saveId )
        {
            return Path.Combine( TimelineMetadata.GetRootDirectory( timelineId ), saveId );
        }

        /// <summary>
        /// Returns the file path to the directory containing the saves of a given timeline.
        /// </summary>
        public static string GetSavesPath( string timelineId )
        {
            return TimelineMetadata.GetRootDirectory( timelineId );
        }

        /// <summary>
        /// Reads all the saves for a given timeline from disk and returns their parsed <see cref="SaveMetadata"/>s.
        /// </summary>
        /// <param name="timelineId">The timeline ID to get the saves for.</param>
        public static IEnumerable<SaveMetadata> ReadAllSaves( string timelineId )
        {
            string savesPath = GetSavesPath( timelineId );

            string[] potentialSaves;
            try
            {
                potentialSaves = Directory.GetDirectories( savesPath );
            }
            catch
            {
                Debug.LogWarning( $"Couldn't open `{savesPath}` directory." );

                return new SaveMetadata[] { };
            }

            List<SaveMetadata> saves = new List<SaveMetadata>();

            foreach( var saveDirPath in potentialSaves )
            {
                try
                {
                    SaveMetadata saveMetadata = new SaveMetadata( timelineId, saveDirPath );
                    saveMetadata.ReadDataFromDisk();
                    saves.Add( saveMetadata );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Couldn't load save `{saveDirPath}`: {ex.Message}" );
                    Debug.LogException( ex );
                }
            }

            return saves;
        }

        public void WriteToDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, SAVE_FILENAME );

            StringBuilder sb = new StringBuilder();
            new JsonStringWriter( this.GetData(), sb ).Write();

            File.WriteAllText( saveFilePath, sb.ToString(), Encoding.UTF8 );
        }

        public void ReadDataFromDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, SAVE_FILENAME );

            string saveJson = File.ReadAllText( saveFilePath, Encoding.UTF8 );

            SerializedData data = new JsonStringReader( saveJson ).Read();

            this.SetData( data );
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
                { "file_version", this.FileVersion.ToString() },
                { "mod_versions", modVersions }
            };
        }
    }
}