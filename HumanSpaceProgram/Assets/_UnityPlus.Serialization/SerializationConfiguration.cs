namespace UnityPlus.Serialization
{
    public enum CycleHandling
    {
        /// <summary>
        /// Automatically resolves cycles by creating a reference (`$ref`) to the previously visited object.
        /// </summary>
        AutoRef,

        /// <summary>
        /// Throws a UPSCircularReferenceException when a cycle is detected.
        /// </summary>
        Throw
    }

    public enum WrapperHandling
    {
        /// <summary>
        /// Allows wrappers even if they don't contain metadata, or are otherwise unnecessary.
        /// </summary>
        Flexible,

        /// <summary>
        /// Throws a UPSInvalidWrapperException if a wrapper is detected but it lacks metadata (`$id` or `$type`).
        /// </summary>
        Strict
    }

    public class SerializationConfiguration
    {
        public ITypeResolver TypeResolver { get; set; } = new DefaultTypeResolver();

        /// <summary>
        /// If true, Collections (Arrays/Lists) are serialized as standard JSON arrays `[...]`. <br/>
        /// If false (default), Collections are wrapped in an object `{"$type": ..., "$id":..., "value": [...]}` to ensure reference integrity.
        /// </summary>
        public bool ForceStandardJson { get; set; } = false;

        /// <summary>
        /// Determines how the serializer handles reference cycles.
        /// </summary>
        public CycleHandling CycleHandling { get; set; } = CycleHandling.Throw;

        /// <summary>
        /// Determines how strict the deserializer should be when encountering `"value"` wrappers.
        /// </summary>
        public WrapperHandling WrapperHandling { get; set; } = WrapperHandling.Strict;

        /// <summary>
        /// The maximum allowed recursion depth. Prevents stack overflows or excessive memory usage from deeply nested data.
        /// </summary>
        public int MaxDepth { get; set; } = 256;


        public static SerializationConfiguration Default => new SerializationConfiguration();
    }
}