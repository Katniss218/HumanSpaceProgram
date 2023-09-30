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

        public TimelineMetadata( string timelineId )
        {
            this.TimelineID = timelineId;
        }

        /// <summary>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </summary>
        public static string GetTimelinesPath()
        {
            return HumanSpaceProgram.GetSaveDirectoryPath();
        }

        /// <summary>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </summary>
        public static string GetRootDirectory( string timelineId )
        {
            return Path.Combine( GetTimelinesPath(), timelineId );
        }
        
        /// <summary>
        /// Returns the path to the (root) directory of the timeline.
        /// </summary>
        public string GetRootDirectory()
        {
            return GetRootDirectory( this.TimelineID );
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

            foreach( var timelineDirName in potentialTimelines )
            {
                try
                {
                    string path = Path.Combine( timelineDirName, TIMELINE_FILENAME );

                    string saveJson = File.ReadAllText( path );

                    SerializedData data = new JsonStringReader( saveJson ).Read();

                    TimelineMetadata timelineMetadata = new TimelineMetadata( timelineDirName );
                    timelineMetadata.SetData( data );
                    timelines.Add( timelineMetadata );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Couldn't load timeline `{timelineDirName}`." );
                    Debug.LogException( ex );
                }
            }

            return timelines;
        }

        public void WriteToDisk()
        {
            string savePath = GetRootDirectory();
            string saveFilePath = Path.Combine( savePath, TIMELINE_FILENAME );

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