namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents a failed member/element that is to be retried later.
    /// </summary>
    /// <typeparam name="T">Either the type of the member/element, or `object` (if inaccessible).</typeparam>
    public class RetryEntry<T>
    {
        public T value;
        public SerializationMapping mapping;

        public RetryEntry( T value, SerializationMapping mapping )
        {
            this.value = value;
            this.mapping = mapping;
        }
    }
}