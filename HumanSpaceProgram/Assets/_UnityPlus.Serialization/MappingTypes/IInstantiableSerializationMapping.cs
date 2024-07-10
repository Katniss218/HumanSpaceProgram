using System;

namespace UnityPlus.Serialization
{
    public interface IInstantiableSerializationMapping
    {
        Func<SerializedData, ILoader, object> OnInstantiate { get; }
    }
}
