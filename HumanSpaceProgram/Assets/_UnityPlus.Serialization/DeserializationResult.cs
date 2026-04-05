namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents the state of deserialization of an *object*.
    /// </summary>
    public enum DeserializationResult
    {
        /// <summary>
        /// Deserialization succeeded, the deserialized object is valid and can be used.
        /// </summary>
        Success,
        /// <summary>
        /// Missing dependency, try again later.
        /// </summary>
        Deferred,
        /// <summary>
        /// Fatal error or data corruption.
        /// </summary>
        Failed
    }
}