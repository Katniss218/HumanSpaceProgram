namespace UnityPlus.Serialization
{
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