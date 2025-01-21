
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

        // Mappings have to use generic `TMember` method param instead of being themselves generic
        // - Because `SerializationMapping<Transform>` can't be cast to `SerializationMapping<Component>`.
        // - And this CAN NOT be solved using variant interfaces - variance is not supported on `ref` parameters.

        /// <summary>
        /// Copies the working data from a mapping definition to a 'work' mapping.
        /// </summary>
        public abstract SerializationMapping GetInstance();

        /// <summary>
        /// Saves the full state of the object.
        /// </summary>
        /// <returns>
        /// True if the member has been fully serialized, false if the method needs to be called again to serialize more.
        /// </returns>
        /// <typeparam name="TMember">Set to the type of the member (field/property) that contains the object.</typeparam>
        public abstract SerializationResult Save<TMember>( TMember obj, ref SerializedData data, ISaver s );

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// True if the member has been fully deserialized, false if the method needs to be called again to deserialize more.
        /// </returns>
        /// <typeparam name="TMember">Set to the type of the member (field/property) that contains the object.</typeparam>
        public abstract SerializationResult Load<TMember>( ref TMember obj, SerializedData data, ILoader l, bool populate );
    }
}