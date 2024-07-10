
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Defines methods for reading and writing serialized data to and from various storage mediums in an abstract way,
    /// such as files, network streams, or memory buffers.
    /// </summary>
    /// <remarks>
    /// Implement this interface if you wish to provide a data handler for a custom storage format.
    /// </remarks>
    public interface ISerializedDataHandler
    {
        /// <summary>
        /// Reads serialized data from the specified source.
        /// </summary>
        /// <returns>The serialized data retrieved from the source.</returns>
        public SerializedData Read();

        /// <summary>
        /// Writes serialized data to the specified source.
        /// </summary>
        /// <param name="data">The serialized data to be written to the source.</param>
        public void Write( SerializedData data );
    }
}