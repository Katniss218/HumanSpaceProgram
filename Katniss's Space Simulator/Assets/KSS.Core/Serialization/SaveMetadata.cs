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
    /// Serializable (meta)data of a timeline's save.
    /// </summary>
    public class SaveMetadata // represents the save file metadata. Not in-timeline data.
    {
        public const string SAVE_FILENAME = "_save.json";

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        string _timelineId;
        string _pathId;

        /// <summary>
        /// Computes the directory path for a given timeline ID.
        /// </summary>
        public static string GetDirectoryPath( string timelineId, string saveId )
        {
            return Path.Combine( HumanSpaceProgram.GetSavesPath(), timelineId, saveId );
        }

        /// <summary>
        /// Creates a new empty <see cref="SaveMetadata"/> that points to the specified save. Does not initialize any display parameters.
        /// </summary>
        /// <param name="path">The path to use to parse out the timeline and save IDs.</param>
        public static SaveMetadata EmptyFromFilePath( string path )
        {
            string[] split = path.Split( new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries );

            int savesIndex = -1;
            for( int i = 0; i < split.Length; i++ )
            {
                if( split[i] == HumanSpaceProgram.SavesDirectoryName )
                {
                    savesIndex = i;
                    break;
                }
            }

            if( savesIndex <= 0 || savesIndex >= split.Length )
            {
                throw new ArgumentException( $"The path `{path}` doesn't contain the `{HumanSpaceProgram.SavesDirectoryName}` directory." );
            }
            if( savesIndex >= split.Length - 2 )
            {
                throw new ArgumentException( $"The path `{path}` points directly to the `{HumanSpaceProgram.SavesDirectoryName}` directory. It must point to a specific save of a specific timeline." );
            }

            return new SaveMetadata()
            {
                _timelineId = split[savesIndex + 1], // `Saves/<timelineId>/<saveId>/_save.json`
                _pathId = split[savesIndex + 2]
            };
        }

        /// <summary>
        /// Reads all the saves for a given timeline from disk and returns their parsed <see cref="SaveMetadata"/>s.
        /// </summary>
        /// <param name="timelineId">The timeline ID to get the saves for.</param>
        public static IEnumerable<SaveMetadata> GetAllSaves( string timelineId )
        {
            string timelinePath = TimelineMetadata.GetDirectoryPath( timelineId );

            string[] potentialSaves;
            try
            {
                potentialSaves = Directory.GetDirectories( timelinePath );
            }
            catch
            {
                Debug.LogWarning( $"Couldn't open saves directory (`{timelinePath}`)." );

                return new SaveMetadata[] { };
            }

            List<SaveMetadata> saves = new List<SaveMetadata>();

            foreach( var save in potentialSaves )
            {
                try
                {
                    string path = Path.Combine( save, SAVE_FILENAME );

                    string saveJson = File.ReadAllText( path );

                    SerializedData data = new JsonStringReader( saveJson ).Read();

                    SaveMetadata timeline = SaveMetadata.EmptyFromFilePath( path );
                    timeline.SetData( data );
                    saves.Add( timeline );
                }
                catch
                {
                    Debug.LogWarning( $"Couldn't load save `{save}`." );
                }
            }

            return saves;
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