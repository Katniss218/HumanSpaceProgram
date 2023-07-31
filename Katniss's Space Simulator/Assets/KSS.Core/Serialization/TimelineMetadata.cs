using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace KSS.Core.Serialization
{
    /// <summary>
    /// Serializable (meta)data of a timeline.
    /// </summary>
    public class TimelineMetadata
    {
        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        string _pathId;

        /// <summary>
        /// Computes the directory path for a given timeline ID.
        /// </summary>
        public static string GetDirectoryPath( string timelineId )
        {
            return Path.Combine( HumanSpaceProgram.GetSavesPath(), timelineId );
        }

        /// <summary>
        /// Creates a new empty <see cref="TimelineMetadata"/> that points to the specified timeline. Does not initialize any display parameters.
        /// </summary>
        /// <param name="path">The path to use to parse out the timeline ID.</param>
        public static TimelineMetadata EmptyFromFilePath( string path )
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
            if( savesIndex >= split.Length - 1 )
            {
                throw new ArgumentException( $"The path `{path}` points directly to the `{HumanSpaceProgram.SavesDirectoryName}` directory. It must point to a specific timeline." );
            }

            return new TimelineMetadata()
            {
                _pathId = split[savesIndex + 1] // `Saves/<timelineId>/<saveId>/_save.json`
            };
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