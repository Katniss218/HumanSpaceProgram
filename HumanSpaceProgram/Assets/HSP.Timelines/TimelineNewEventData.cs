using HSP.Timelines.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Timelines
{
    public sealed class TimelineNewEventData : IMessageEventData
    {
        public readonly ScenarioMetadata scenario;
        public readonly TimelineMetadata timeline;

        private readonly Dictionary<LogType, List<string>> _messages = new();

        public bool HasErrors => _messages.ContainsKey( LogType.Error ) || _messages.ContainsKey( LogType.Exception );

        public TimelineNewEventData( ScenarioMetadata scenario, TimelineMetadata timeline )
        {
            this.scenario = scenario;
            this.timeline = timeline;
        }

        public IEnumerable<string> GetMessages( LogType severity )
        {
            if( !_messages.ContainsKey( severity ) )
                return new List<string>();

            return _messages[severity];
        }

        public void AddMessage( LogType severity, string message )
        {
            if( !_messages.ContainsKey( severity ) )
                _messages[severity] = new();

            _messages[severity].Add( message );
        }
    }
}