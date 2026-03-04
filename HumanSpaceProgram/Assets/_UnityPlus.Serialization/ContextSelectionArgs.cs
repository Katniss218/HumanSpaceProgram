using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Arguments passed to an IContextSelector to determine the context for a child element.
    /// </summary>
    public readonly struct ContextSelectionArgs
    {
        /// <summary>
        /// The index of the child element in the collection (or tuple).
        /// </summary>
        public readonly int Index { get; }

        /// <summary>
        /// The Key identifier for this element.
        /// <para>For Arrays: Null.</para>
        /// <para>For Objects: The Member Name (string).</para>
        /// </summary>
        public readonly string Key { get; }

        /// <summary>
        /// The declared type of the storage location (e.g., T in List<T>).
        /// This is the type assumed if no polymorphic header is present.
        /// </summary>
        public readonly Type DeclaredType { get; }

        /// <summary>
        /// The actual type of the object instance.
        /// <para>On Serialization: The type of the live instance (`instance.GetType()`).</para>
        /// <para>On Deserialization: The resolved type from the `$type` header. Equal <see cref="DeclaredType"/> when no header exists.</para>
        /// </summary>
        public readonly Type ActualType { get; }

        /// <summary>
        /// The total number of elements in the parent container. 
        /// (-1 if not applicable).
        /// </summary>
        public readonly int ContainerCount { get; }

        public ContextSelectionArgs( string key, Type declaredType, Type actualType, int containerCount )
        {
            Index = -1;
            Key = key;
            DeclaredType = declaredType;
            ActualType = actualType ?? declaredType;
            ContainerCount = containerCount;
        }

        public ContextSelectionArgs( int index, Type declaredType, Type actualType, int containerCount )
        {
            Index = index;
            Key = null;
            DeclaredType = declaredType;
            ActualType = actualType ?? declaredType;
            ContainerCount = containerCount;
        }
    }
}