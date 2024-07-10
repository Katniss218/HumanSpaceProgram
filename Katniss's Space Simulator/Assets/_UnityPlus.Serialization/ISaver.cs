using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents an abstract functionality that can save a collection of objects while persisting their references.
    /// </summary>
    public interface ISaver
    {
        //public delegate void Action( IReverseReferenceMap s );

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
        IReverseReferenceMap RefMap { get; }
    }

    /// <summary>
    /// <see cref="ISaver"/>, but can save over multiple frames.
    /// </summary>
    public interface IAsyncSaver : ISaver
    {
        //new public delegate IEnumerator Action( IReverseReferenceMap s );

        /// <summary>
        /// The percentage (in [0..1]) of completion of the current action (0 = 0% completed, 1 = 100% completed).
        /// </summary>
        /// <remarks>
        /// Intended to be set by the serialization strategy, and not by the saver.
        /// </remarks>
        float CurrentActionPercentCompleted { get; set; }
        /// <summary>
        /// The percentage (in [0..1]) of completion of the entire saving process (0 = 0% completed, 1 = 100% completed).
        /// </summary>
        float TotalPercentCompleted { get; }
    }
}