using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a Node that contains other nodes (Class, Struct, Array).
    /// These require pushing a new frame to the Stack Machine.
    /// </summary>
    public interface ICompositeDescriptor : IDescriptor
    {
        // Immutable / Factory Support

        /// <summary>
        /// The number of steps that represent Constructor Arguments.
        /// These steps are executed *before* the real object is constructed.
        /// For dynamic types (Delegates), this can depend on the initial target buffer.
        /// </summary>
        int GetConstructionStepCount( object target );

        /// <summary>
        /// Converts the initial target (e.g. object[] buffer) into the final instance.
        /// For mutable objects, this simply returns the target.
        /// </summary>
        object Construct( object initialTarget );

        // Population Support

        /// <summary>
        /// Returns the number of steps (members/elements) required to process this object.
        /// </summary>
        int GetStepCount( object target );

        /// <summary>
        /// Gets the descriptor for a specific step (index).
        /// </summary>
        IMemberInfo GetMemberInfo( int stepIndex );

        /// <summary>
        /// Returns an enumerator for members, enabling O(N) serialization for collections.
        /// Returns null if random access (GetMemberInfo) should be used.
        /// </summary>
        IEnumerator<IMemberInfo> GetMemberEnumerator( object target );

        // Methods

        int GetMethodCount();
        IMethodInfo GetMethodInfo( int methodIndex );

        // Lifecycle Callbacks

        void OnSerializing( object target, SerializationContext context );
        void OnDeserialized( object target, SerializationContext context );
    }
}