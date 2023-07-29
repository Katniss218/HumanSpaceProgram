using System;

namespace UnityPlus.OverridableEvents
{
    /// <summary>
    /// Represents a generic event listener that can block other listeners from executing.
    /// </summary>
    /// <typeparam name="T">The delegate that this listener uses to respond to the event.</typeparam>
    public struct OverridableEventListener<T>
    {
        /// <summary>
        /// the ID of the listener.
        /// </summary>
        public string id;
        /// <summary>
        /// The IDs of other listeners that this listener blocks from executing.
        /// </summary>
        public string[] blacklist;
        /// <summary>
        /// The delegate that this listener responds with.
        /// </summary>
        public T func;
    }
}