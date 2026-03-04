namespace UnityPlus.Serialization
{
    public enum SerializationCursorResult
    {
        /// <summary>
        /// Move to the next step (Increment StepIndex).
        /// </summary>
        Advance,

        /// <summary>
        /// Keep the current StepIndex (used for Phase changes or Retry).
        /// </summary>
        Jump,

        /// <summary>
        /// Push a child cursor. The Driver will increment the Parent's StepIndex automatically.
        /// </summary>
        Push,

        /// <summary>
        /// Pop the current cursor (Finished processing).
        /// </summary>
        Finished,

        /// <summary>
        /// The current cursor cannot proceed due to missing deps; pop it (it's queued).
        /// </summary>
        Deferred
    }
}