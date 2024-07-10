using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Json;

namespace HSP.Core.Serialization
{
    /// <summary>
    /// Serializable (meta)data of a timeline.
    /// </summary>
    public sealed class TimelineMetadata
    {
        /// <summary>
        /// The name of the file that stores the timeline metadata.
        /// </summary>
        public const string TIMELINE_FILENAME = "_timeline.json";

        /// <summary>
        /// The unique ID of this specific timeline.
        /// </summary>
        public readonly string TimelineID;

        /// <summary>
        /// The display name shown in the GUI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description shown in the GUI.
        /// </summary>
        public string Description { get; set; }

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
                    TimelineMetadata timelineMetadata = TimelineMetadata.LoadFromDisk( timelineDirName );
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

        public static TimelineMetadata LoadFromDisk( string timelineId )
        {
            string saveFilePath = Path.Combine( GetRootDirectory( timelineId ), TIMELINE_FILENAME );

            TimelineMetadata timelineMetadata = new TimelineMetadata( timelineId );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( saveFilePath );
            var data = handler.Read();
            SerializationUnit.Populate( timelineMetadata, data );
            return timelineMetadata;
        }

        public void SaveToDisk()
        {
            string saveFilePath = Path.Combine( GetRootDirectory(), TIMELINE_FILENAME );

            var data = SerializationUnit.Serialize( this );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( saveFilePath );
            handler.Write( data );
        }

        [MapsInheritingFrom( typeof( TimelineMetadata ) )]
        public static SerializationMapping TimelineMetadataMapping()
        {
            return new MemberwiseSerializationMapping<TimelineMetadata>()
            {
                ("name", new Member<TimelineMetadata, string>( o => o.Name )),
                ("description", new Member<TimelineMetadata, string>( o => o.Description ))
            };
        }
    }
}