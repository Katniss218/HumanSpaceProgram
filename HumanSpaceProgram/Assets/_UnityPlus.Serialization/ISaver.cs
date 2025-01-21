
namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents an abstract functionality that can save a collection of objects while persisting their references.
    /// </summary>
    public interface ISaver
    {
        /// <summary>
        /// The current state of the saver. <br />
        /// Saving is split into 2 stages - see the enum values.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// Not saving.
            /// </summary>
            Idle = 0,

            /// <summary>
            /// 1. Save object data.
            ///    Loop through every object and save its persistent data.
            ///    When serializing a reference, ask what the ID of that object is by passing the object to the `RegisterOrGet` method.
            /// </summary>
            Saving,
        }

        /// <summary>
        /// The reference map used to map object IDs to references when serializing.
        /// </summary>
        public IReverseReferenceMap RefMap { get; }

        public int CurrentPass { get; }

        public bool ShouldPause();
    }
}