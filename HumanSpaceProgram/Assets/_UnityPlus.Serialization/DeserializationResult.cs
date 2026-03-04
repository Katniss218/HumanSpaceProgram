namespace UnityPlus.Serialization
{
    public enum DeserializationResult
    {
        Success,
        Deferred, // Missing dependency, try again later
        Failed    // Fatal error or data corruption
    }
}