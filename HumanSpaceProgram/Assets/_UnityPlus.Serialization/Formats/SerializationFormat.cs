using System.IO;

namespace UnityPlus.Serialization.Formats
{
    /// <summary>
    /// Defines a strategy for converting between a Stream and SerializedData.
    /// Decouples the Data Format from the Storage Medium.
    /// </summary>
    public interface ISerializationFormat
    {
        SerializedData Read( Stream stream );
        void Write( Stream stream, SerializedData data );
    }
}