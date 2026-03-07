using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public interface IReadonlySerializationContext
    {
        SerializationConfiguration Config { get; }

        IForwardReferenceMap ForwardMap { get; }
        IReverseReferenceMap ReverseMap { get; }

        SerializationLog Log { get; }
    }

    /// <summary>
    /// Represents the mutable session state of a serialization operation.
    /// </summary>
    public class SerializationContext : IReadonlySerializationContext
    {
        public SerializationConfiguration Config { get; }

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        public IForwardReferenceMap ForwardMap { get; set; }
        /// <summary>
        /// Used for serialization.
        /// </summary>
        public IReverseReferenceMap ReverseMap { get; set; }

        /// <summary>
        /// Collects errors, warnings, and info logs generated during the operation.
        /// </summary>
        public SerializationLog Log { get; } = new SerializationLog();

        /// <summary>
        /// Holds operations that failed due to missing dependencies.
        /// They will be retried after the main stack is cleared.
        /// </summary>
        public Queue<DeferredOperation> DeferredOperations { get; private set; } = new Queue<DeferredOperation>();

        public SerializationContext( SerializationConfiguration config )
        {
            Config = config ?? new SerializationConfiguration();
        }

        public SerializationContext()
        {
            Config = new SerializationConfiguration();
        }

        public void EnqueueDeferred( object target, IMemberInfo member, SerializedData data )
        {
            DeferredOperations.Enqueue( new DeferredOperation
            {
                Target = target,
                Member = member,
                Data = data,
                Descriptor = member.TypeDescriptor
            } );
        }

        public void EnqueueDeferredRoot( IDescriptor descriptor, SerializedData data )
        {
            DeferredOperations.Enqueue( new DeferredOperation
            {
                Target = null,
                Member = null,
                Data = data,
                Descriptor = descriptor
            } );
        }
    }
}