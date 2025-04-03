using HSP.Content;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP.Timelines.Serialization
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
        /// The unique ID of the scenario associated with this timeline.
        /// </summary>
        public NamespacedID ScenarioID;

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
            if( string.IsNullOrEmpty( timelineId ) )
                throw new ArgumentNullException( nameof( timelineId ) );

            this.TimelineID = timelineId;
        }

        /// <summary>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </summary>
        public static string GetTimelinesPath()
        {
            return HumanSpaceProgramContent.GetSaveDirectoryPath();
        }

        /// <summary>
        /// Returns the path to the (root) directory of the timeline.
        /// </summary>
        /// <remarks>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </remarks>
        public static string GetRootDirectory( string timelineId )
        {
            // Saves/<timeline_id>/...
            return Path.Combine( GetTimelinesPath(), timelineId );
        }

        /// <summary>
        /// Returns the path to the (root) directory of the timeline.
        /// </summary>
        /// <remarks>
        /// Root directory is the directory that contains the _timeline.json file.
        /// </remarks>
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

            foreach( var timelineDirPath in potentialTimelines )
            {
                string timelineId = Path.GetRelativePath( timelinesDirectory, timelineDirPath );
                try
                {
                    TimelineMetadata timelineMetadata = TimelineMetadata.LoadFromDisk( timelineId );
                    timelines.Add( timelineMetadata );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Couldn't load timeline `{timelineDirPath}`." );
                    Debug.LogException( ex );
                }
            }

            return timelines;
        }

        public static TimelineMetadata LoadFromDisk( string timelineId )
        {
            string filePath = Path.Combine( GetRootDirectory( timelineId ), TIMELINE_FILENAME );

            TimelineMetadata timelineMetadata = new TimelineMetadata( timelineId );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( filePath );
            var data = handler.Read();
            SerializationUnit.Populate( timelineMetadata, data );
            return timelineMetadata;
        }

        public void SaveToDisk()
        {
            string filePath = Path.Combine( GetRootDirectory(), TIMELINE_FILENAME );

            var data = SerializationUnit.Serialize( this );

            JsonSerializedDataHandler handler = new JsonSerializedDataHandler( filePath );
            handler.Write( data );
        }

        [MapsInheritingFrom( typeof( TimelineMetadata ) )]
        public static SerializationMapping TimelineMetadataMapping()
        {
            return new MemberwiseSerializationMapping<TimelineMetadata>()
                .WithMember( "scenario_id", o => o.ScenarioID )
                .WithMember( "name", o => o.Name )
                .WithMember( "description", o => o.Description );
        }
    }
}