using HSP.Timelines.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Timelines
{
    public sealed class TimelineSaveEventData : IMessageEventData
    {
        public readonly ScenarioMetadata scenario;
        public readonly TimelineMetadata timeline;
        public readonly SaveMetadata save;

        private readonly Dictionary<LogType, List<string>> _messages = new();

        public bool HasErrors => _messages.ContainsKey( LogType.Error ) || _messages.ContainsKey( LogType.Exception );

        public TimelineSaveEventData( ScenarioMetadata scenario, TimelineMetadata timeline, SaveMetadata save )
        {
            this.scenario = scenario;
            this.timeline = timeline;
            this.save = save;
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