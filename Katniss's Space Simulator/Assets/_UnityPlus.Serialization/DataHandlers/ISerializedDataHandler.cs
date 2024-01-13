
namespace UnityPlus.Serialization.DataHandlers
{
    /// <summary>
    /// Handles reading and writing serialized data to an abstract source (file, network, memory, etc).
    /// </summary>
    public interface ISerializedDataHandler
    {
        /// <summary>
        /// Reads the serialized objects and data from the source.
        /// </summary>
        /// <returns>The serialized objects and data.</returns>
        public (SerializedData o, SerializedData d) ReadObjectsAndData();

        /// <summary>
        /// Writes the serialized objects and data to the source.
        /// </summary>
        public void WriteObjectsAndData( SerializedData o, SerializedData d );
    }
}