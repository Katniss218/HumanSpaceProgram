using System;
using System.Runtime.Serialization;

namespace HSP.Timelines
{
    /// <summary>
    /// Exception thrown when a specific save cannot be found.
    /// </summary>
    public class SaveNotFoundException : Exception
    {
        public string TimelineID { get; }
        public string SaveID { get; }

        public SaveNotFoundException( string timelineId, string saveId )
            : base( $"The save {saveId} could not be found." )
        {
            this.TimelineID = timelineId;
            this.SaveID = saveId;
        }

        public SaveNotFoundException( string timelineId, string saveId, string message )
            : base( message )
        {
            this.TimelineID = timelineId;
            this.SaveID = saveId;
        }

        public SaveNotFoundException( string timelineId, string saveId, Exception innerException )
            : base( $"The save {saveId} could not be found.", innerException )
        {
            this.TimelineID = timelineId;
            this.SaveID = saveId;
        }

        public SaveNotFoundException( string timelineId, string saveId, string message, Exception innerException )
            : base( message, innerException )
        {
            this.TimelineID = timelineId;
            this.SaveID = saveId;
        }



        protected SaveNotFoundException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}