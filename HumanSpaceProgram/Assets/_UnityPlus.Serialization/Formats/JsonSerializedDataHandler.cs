using System;

namespace UnityPlus.Serialization.Formats
{
    /// <summary>
    /// Specialized handler for JSON files. This is for backwards compatibility.
    /// </summary>
    [Obsolete( "Use FileSerializedDataHandler with JsonFormat.Instance instead." )]
    public class JsonSerializedDataHandler : FileSerializedDataHandler
    {
        public JsonSerializedDataHandler( string filepath ) : base( filepath, JsonFormat.Instance )
        {
        }
    }
}