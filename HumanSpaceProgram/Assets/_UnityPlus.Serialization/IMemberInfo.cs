using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a specific step in the serialization process (a Field, Property, or Array Element).
    /// </summary>
    public interface IMemberInfo
    {
        /// <summary>
        /// The name of the member (key in JSON object). Null if not applicable (collection elements).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The index of the member (used for Arrays/Lists). -1 if not applicable (named members).
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The C# type associated with this member ("storage location", not instance).
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// The descriptor that handles this member's declared type.
        /// <para>
        /// This should return the descriptor for <see cref="MemberType"/>, NOT the runtime type of the value.
        /// Polymorphism is handled by the SerializationStrategy.
        /// </para>
        /// </summary>
        IDescriptor TypeDescriptor { get; } // cached here for speed. Assigned from the parent descriptor when the member info is created.

        /// <summary>
        /// If true, modifying the value returned by GetValue requires a Write-Back to persist changes (e.g. Structs).
        /// </summary>
        bool RequiresWriteBack { get; }

        /// <summary>
        /// The serialization context used to resolve the descriptor for this member.
        /// </summary>
        ContextKey GetContext( object target );

        /// <summary>
        /// Retrieves the value from the target object.
        /// </summary>
        object GetValue( object target );

        /// <summary>
        /// Sets the value on the target object.
        /// </summary>
        /// <param name="target">The target object. Passed by ref to support replacing boxed value types.</param>
        /// <param name="value">The value to set.</param>
        void SetValue( ref object target, object value );
    }
}