using System;
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
            SavingData,

            /// <summary>
            /// 2. Save object instances (references).
            ///    Loop through these objects again, and save them, along with their IDs (if referenced by anything).
            ///    Use the object registry to get the IDs of objects that have been assigned to them in step 1.
            /// </summary>
            SavingObjects

            // This setup lets us know what objects are referenced by something before we start saving those potentially-referenced objects.
            // It also lets us decouple the instances from their data.
        }

        bool TryGetID( object obj, out Guid id );

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) under a random ID, and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
        Guid GetReferenceID( object obj );

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) under a specific ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference. <br />
        /// For reference safety, the user should always use the return value instead of the input parameter, because the object might've been already registered under a different ID.
        /// </remarks>
        Guid GetReferenceID( object obj, Guid guid );
    }

    /// <summary>
    /// <see cref="ISaver"/>, but can save over multiple frames.
    /// </summary>
    public interface IAsyncSaver : ISaver
    {
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