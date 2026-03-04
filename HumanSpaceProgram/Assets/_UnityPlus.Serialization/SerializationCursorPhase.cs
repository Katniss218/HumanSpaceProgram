namespace UnityPlus.Serialization
{
    public enum SerializationCursorPhase
    {
        // Deserialization happens in several distinct phases.

        /// <summary>
        /// Initial setup. Resolving types, allocating buffers, calling OnSerializing.
        /// </summary>
        PreProcessing,

        /// <summary>
        /// Gathering arguments for the constructor. Target is null (or buffer).
        /// </summary>
        /// <remarks>
        /// Only used for deserialization, and only for objects which are at least partially immutable.
        /// </remarks>
        Construction,

        /// <summary>
        /// Creating the actual instance from the gathered arguments.
        /// </summary>
        /// <remarks>
        /// Only used for deserialization.
        /// </remarks>
        Instantiation,

        /// <summary>
        /// Setting members on the instantiated object. Target is valid.
        /// </summary>
        Population,

        /// <summary>
        /// Final cleanup and callbacks (OnDeserialized).
        /// </summary>
        PostProcessing
    }
}