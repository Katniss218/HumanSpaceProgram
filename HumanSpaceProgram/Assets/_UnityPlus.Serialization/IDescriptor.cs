using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Base interface for all type descriptors.
    /// </summary>
    public interface IDescriptor
    {
        Type MappedType { get; }

        /// <summary>
        /// Creates the initial object for serialization. 
        /// For mutable types, this is the instance itself.
        /// For immutable types (constructor injection), this is an object[] buffer.
        /// </summary>
        object CreateInitialTarget( SerializedData data, SerializationContext ctx );
    }    
}