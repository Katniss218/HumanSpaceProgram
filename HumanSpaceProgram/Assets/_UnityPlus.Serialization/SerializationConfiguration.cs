namespace UnityPlus.Serialization
{
    public class SerializationConfiguration
    {
        public ITypeResolver TypeResolver { get; set; } = new DefaultTypeResolver();

        /// <summary>
        /// If true, Collections (Arrays/Lists) are serialized as standard JSON arrays `[...]`. <br/>
        /// If false (default), Collections are wrapped in an object `{"$type": ..., "$id":..., "value": [...]}` to ensure reference integrity.
        /// </summary>
        public bool ForceStandardJson { get; set; } = false;
    }
}