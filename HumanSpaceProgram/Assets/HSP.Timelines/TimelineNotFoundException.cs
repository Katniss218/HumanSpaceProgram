using System;
using System.Runtime.Serialization;

namespace HSP.Timelines
{
    /// <summary>
    /// Exception thrown when a specific timeline cannot be found.
    /// </summary>
    public class TimelineNotFoundException : Exception
    {
        public string TimelineID { get; }

        public TimelineNotFoundException( string timelineId )
            : base( $"The timeline {timelineId} could not be found." )
        {
            this.TimelineID = timelineId;
        }

        public TimelineNotFoundException( string timelineId, string message )
            : base( message )
        {
            this.TimelineID = timelineId;
        }

        public TimelineNotFoundException( string timelineId, Exception innerException )
            : base( $"The timeline {timelineId} could not be found.", innerException )
        {
            this.TimelineID = timelineId;
        }

        public TimelineNotFoundException( string timelineId, string message, Exception innerException )
            : base( message, innerException )
        {
            this.TimelineID = timelineId;
        }



        protected TimelineNotFoundException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}