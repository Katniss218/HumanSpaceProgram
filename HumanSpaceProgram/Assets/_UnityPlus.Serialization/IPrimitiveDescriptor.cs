namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a Leaf node (int, string, float, Guid).
    /// These are processed inline and do NOT push a stack frame.
    /// </summary>
    public interface IPrimitiveDescriptor : IDescriptor
    {
        void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx );
        DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result );
    }
}