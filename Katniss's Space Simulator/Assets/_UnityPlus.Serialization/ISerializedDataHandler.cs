
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Handles reading and writing serialized data to an abstract source (file, network, memory, etc).
    /// </summary>
    public interface ISerializedDataHandler
    {
        public SerializedData Read();

        public void Write( SerializedData data );
    }
}