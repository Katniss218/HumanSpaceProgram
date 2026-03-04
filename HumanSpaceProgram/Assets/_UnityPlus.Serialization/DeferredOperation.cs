namespace UnityPlus.Serialization
{
    public struct DeferredOperation
    {
        public object Target;       // The Parent Object (or Parent Buffer). Null if this is a Root Object deferral.
        public IMemberInfo Member;  // The Member definition. Null if Root.
        public SerializedData Data; // The data required to build the child value
        public IDescriptor Descriptor; // The descriptor for the value

        // State for resuming an interrupted construction
        public object[] ConstructionBuffer;
        public int ConstructionIndex;
    }
}
