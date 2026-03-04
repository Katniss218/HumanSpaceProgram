namespace UnityPlus.Serialization
{
    public interface IOperationStrategy
    {
        /// <summary>
        /// Sets up the initial stack state and root result logic.
        /// </summary>
        void InitializeRoot( object root, IDescriptor descriptor, SerializedData data, SerializationState state );

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