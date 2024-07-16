using System;

namespace UnityPlus.OverridableEvents
{
    /// <summary>
    /// Represents a generic event listener that can block other listeners from executing.
    /// </summary>
    /// <typeparam name="T">The delegate that this listener uses to respond to the event.</typeparam>
    public class OverridableEventListener<T> : IOverridable<string>, ITopologicallySortable<string>
    {
        /// <summary>
        /// The ID of this listener.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The IDs of other listeners that this listener blocks.
        /// </summary>
        public string[] Blacklist { get; }

        /// <summary>
        /// The listener will run BEFORE the specified listeners (unless they're blocked).
        /// </summary>
        public string[] Before { get; }

        /// <summary>
        /// The listener will run AFTER the specified listeners (unless they're blocked).
        /// </summary>
        public string[] After { get; }

        /// <summary>
        /// The delegate that this listener invokes.
        /// </summary>
        public Action<T> OnInvoke { get; set; }

        public OverridableEventListener( string id, string[] blacklist, string[] before, string[] after, Action<T> onInvoke )
        {
            this.ID = id;
            this.Blacklist = blacklist ?? new string[] { };
            this.Before = before ?? new string[] { };
            this.After = after ?? new string[] { };
            this.OnInvoke = onInvoke;
        }

        public OverridableEventListener( string id, string[] blacklist, Action<T> onInvoke )
        {
            this.ID = id;
            this.Blacklist = blacklist ?? new string[] { };
            this.Before = new string[] { };
            this.After = new string[] { };
            this.OnInvoke = onInvoke;
        }

        public OverridableEventListener( string id, Action<T> onInvoke )
        {
            this.ID = id;
            this.Blacklist = new string[] { };
            this.Before = new string[] { };
            this.After = new string[] { };
            this.OnInvoke = onInvoke;
        }
    }
}