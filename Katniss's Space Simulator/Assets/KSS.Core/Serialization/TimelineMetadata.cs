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
    /// Serializable (meta)data of a timeline.
    /// </summary>
    public sealed class TimelineMetadata
    {
        public const string TIMELINE_FILENAME = "_timeline.json";

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The unique ID of this specific timeline.
        /// </summary>
        public readonly string TimelineID;

        TimelineMetadata( string timelineId )
        {
            this.TimelineID = timelineId;
        }

        /// <summary>
        /// Returns the file path to the directory containing the timelines.
        /// </summary>
        public static string GetTimelinesPath()
        {
            return HumanSpaceProgram.GetSaveDirectoryPath();
        }

        /// <summary>
        /// Returns the file path to the directory containing the saves of a given timeline.
        /// </summary>
        public static string GetSavesPath( string timelineId )
        {
            return Path.Combine( GetTimelinesPath(), timelineId );
        }

        /// <summary>
        /// Creates a new empty <see cref="TimelineMetadata"/> that points to the specified timeline. Does not initialize any display parameters.
        /// </summary>
        /// <param name="validPath">The path to use to parse out the timeline ID.</param>
        public static TimelineMetadata EmptyFromFilePath( string validPath )
        {
            string[] split = validPath.Split( new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries );

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
                throw new ArgumentException( $"The path `{validPath}` doesn't contain the `{HumanSpaceProgram.SavesDirectoryName}` directory." );
            }
            if( savesIndex >= split.Length - 1 )
            {
                throw new ArgumentException( $"The path `{validPath}` points directly to the `{HumanSpaceProgram.SavesDirectoryName}` directory. It must point to a specific timeline." );
            }

            return new TimelineMetadata( split[savesIndex + 1] ); // `Saves/<timelineId>/<saveId>/_save.json`
        }

        /// <summary>
        /// Reads all timelines from disk and returns a list of their metadata.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TimelineMetadata> ReadAllTimelines()
        {
            string timelinesDirectory = GetTimelinesPath();

            string[] potentialTimelines;
            try
            {
                potentialTimelines = Directory.GetDirectories( timelinesDirectory );
            }
            catch
            {
                Debug.LogWarning( $"Couldn't open `{timelinesDirectory}` directory." );

                return new TimelineMetadata[] { };
            }

            List<TimelineMetadata> timelines = new List<TimelineMetadata>();

            foreach( var timeline in potentialTimelines )
            {
                try
                {
                    string path = Path.Combine( timeline, TIMELINE_FILENAME );

                    string saveJson = File.ReadAllText( path );

                    SerializedData data = new JsonStringReader( saveJson ).Read();

                    TimelineMetadata timelineMetadata = TimelineMetadata.EmptyFromFilePath( path );
                    timelineMetadata.SetData( data );
                    timelines.Add( timelineMetadata );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Couldn't load timeline `{timeline}`." );
                    Debug.LogException( ex );
                }
            }

            return timelines;
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