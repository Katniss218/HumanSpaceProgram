using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        /// <summary>
        /// Serialization must be aborted and cleaned up.
        /// </summary>
        Fatal
    }

    public struct LogEntry
    {
        public LogLevel Level;
        public string Message;
        public string ObjectPath;
        public object ContextObject;
        public SerializedData ContextNode;

        public override string ToString() => $"[{Level}] {ObjectPath}: {Message}";
    }

    public class SerializationLog
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        public bool HasFatalErrors { get; private set; }

        public IReadOnlyList<LogEntry> Logs => _logs;

        // Optional hook for external logging systems (e.g. Unity Console)
        public Action<LogEntry> OnLog;

        public void Log( LogLevel level, string message, SerializationState state = null, object target = null )
        {
            if( level == LogLevel.Fatal ) HasFatalErrors = true;

            string path = "Unknown";
            if( state != null )
            {
                path = state.Stack.BuildPath();
            }

            var entry = new LogEntry
            {
                Level = level,
                Message = message,
                ObjectPath = path,
                ContextObject = target
            };

            _logs.Add( entry );

            OnLog?.Invoke( entry );
        }

        public void Clear()
        {
            _logs.Clear();
            HasFatalErrors = false;
        }
    }
}