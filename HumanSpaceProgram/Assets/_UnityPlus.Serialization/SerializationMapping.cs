
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents an arbitrary serialization mapping.
    /// </summary>
    public abstract class SerializationMapping
    {
        /// <summary>
        /// Gets the serialization context in which this mapping operates.
        /// </summary>
        public int Context { get; internal set; }

        // Mappings use generic `T` instead of being themselves generic - because `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`.
        // - This CAN NOT be solved using variant interfaces - variance is not supported on `ref` parameters.

        /// <summary>
        /// Override this if your mapping contains any additional data (and return copy of the mapping, but with those additional data fields cleared).
        /// </summary>
        /// <returns>Either "this", or a clone (depending on if the mapping needs to persist data between Load and LoadReferences).</returns>
        public abstract SerializationMapping GetInstance();

        /// <summary>
        /// Saves the full state of the object.
        /// </summary>
        /// <returns>
        /// True if the member has been fully serialized, false if the method needs to be called again to serialize more.
        /// </returns>
        public abstract MappingResult Save<T>( T obj, ref SerializedData data, ISaver s );

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// True if the member has been fully deserialized, false if the method needs to be called again to deserialize more.
        /// </returns>
        public abstract MappingResult Load<T>( ref T obj, SerializedData data, ILoader l, bool populate );
    }
}