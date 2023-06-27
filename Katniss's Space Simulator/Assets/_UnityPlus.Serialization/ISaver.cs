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
        /// Saving is split into 2 stages.
        /// </summary>
        public enum State : byte
        {
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
        /// Registers the specified object in the registry (if not registered already) and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
        Guid GetID( object obj );
    }

    /// <summary>
    /// <see cref="ISaver"/>, but can save over multiple frames.
    /// </summary>
    public interface IAsyncSaver : ISaver
    {
        float CurrentActionPercentCompleted { get; set; }
        float TotalPercentCompleted { get; }
    }
}