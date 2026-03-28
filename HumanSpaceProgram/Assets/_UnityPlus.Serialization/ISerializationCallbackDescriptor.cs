namespace UnityPlus.Serialization
{
    /// <summary>
    /// Interface for descriptors that support serialization lifecycle callbacks.
    /// </summary>
    public interface ISerializationCallbackDescriptor
    {
        void OnSerializing( object target, SerializationContext ctx );
        void OnSerialized( object target, SerializationContext ctx );
        void OnDeserializing( object target, SerializationContext ctx );
        void OnDeserialized( object target, SerializationContext ctx );
    }
}