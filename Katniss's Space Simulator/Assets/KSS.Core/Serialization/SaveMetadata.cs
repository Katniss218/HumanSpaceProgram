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
        public const string SAVE_FILENAME = "_save.json";

        /// <summary>
        /// The persistent save's ID. A persistent save is the default save when a custom save is not specified.
        /// </summary>
        public const string PERSISTENT_SAVE_ID = "___persistent";

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The unique ID of this specific save's timeline.
        /// </summary>
        public readonly string TimelineID;
        /// <summary>
        /// The unique ID of this specific save.
        /// </summary>
        public readonly string SaveID;

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

            foreach( var saveDirName in potentialSaves )
            {
                try
                {
                    string path = Path.Combine( saveDirName, SAVE_FILENAME );

                    string saveJson = File.ReadAllText( path );

                    SerializedData data = new JsonStringReader( saveJson ).Read();

                    SaveMetadata saveMetadata = new SaveMetadata( timelineId, saveDirName );
                    saveMetadata.SetData( data );
                    saves.Add( saveMetadata );
                }
                catch
                {
                    Debug.LogWarning( $"Couldn't load save `{saveDirName}`." );
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

        public void SetData( SerializedData data )
        {
            this.Name = data["name"];
            this.Description = data["description"];
        }

        public SerializedData GetData()
        {
            return new SerializedObject()
            {
                { "name", this.Name },
                { "description", this.Description }
            };
        }
    }
}