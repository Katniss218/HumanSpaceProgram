using HSP.Content;
using System;
using System.Runtime.Serialization;

namespace HSP.Timelines
{
    /// <summary>
    /// Exception thrown when a specific scenario cannot be found.
    /// </summary>
    public class ScenarioNotFoundException : Exception
    {
        public NamespacedID ScenarioID { get; }

        public ScenarioNotFoundException( NamespacedID scenarioId )
            : base( $"The scenario {scenarioId} could not be found." )
        {
            this.ScenarioID = scenarioId;
        }

        public ScenarioNotFoundException( NamespacedID scenarioId, string message )
            : base( message )
        {
            this.ScenarioID = scenarioId;
        }

        public ScenarioNotFoundException( NamespacedID scenarioId, Exception innerException )
            : base( $"The scenario {scenarioId} could not be found.", innerException )
        {
            this.ScenarioID = scenarioId;
        }

        public ScenarioNotFoundException( NamespacedID scenarioId, string message, Exception innerException )
            : base( message, innerException )
        {
            this.ScenarioID = scenarioId;
        }



        protected ScenarioNotFoundException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }
}