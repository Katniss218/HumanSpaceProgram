using System;

namespace UnityPlus.Serialization
{
    public interface IOperationStrategy
    {
        /// <summary>
        /// Sets up the initial stack state and root result logic.
        /// </summary>
        void InitializeRoot( Type declaredType, ContextKey contextKey, object root, SerializedData data, SerializationState state );

        /// <summary>
        /// Processes the current cursor.
        /// </summary>
        SerializationCursorResult Process( ref SerializationCursor cursor, SerializationState state );

        /// <summary>
        /// Called when a cursor finishes (is popped from the stack).
        /// </summary>
        void OnCursorFinished( SerializationCursor cursor, SerializationState state );
    }
}