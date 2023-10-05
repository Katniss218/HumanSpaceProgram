using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents an abstract functionality that can load a collection of objects while persisting their references.
    /// </summary>
    public interface ILoader
    {
        /// <summary>
        /// The current state of the loader. <br />
        /// Loading is split into 2 stages - see the enum values.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// Not loading.
            /// </summary>
            Idle = 0,

            /// <summary>
            /// 1. Creation of referencable objects. <br />
            ///    These objects will have default parameters, and can can be created by a factory, or a number of other methods. <br />
            ///    This step includes deserializing other save-specific items, such as dialogues (if applicable).
            /// </summary>
            LoadingObjects,

            /// <summary>
            /// 2. Applying data to the created objects.  <br />
            ///    After every referencable object has been created, we can load the things that reference them. In practice, this means we apply *all* data after everything has been created.
            /// </summary>
            LoadingData

            // This setup disallows reading references while the objects are being created,
            // but lets us be sure that when we start reading them later, every reference that can be referenced will exist.

            // It lets us do that without hacking some system together, that loads objects as the references are resolved, and also allows circular referencing.
        }

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        /// <remarks>
        /// Call this method when loading an object that might be referenced. <br />
        /// Implementations should throw an exception if the object is already registered.
        /// </remarks>
        void SetReferenceID( object obj, Guid id );

        /// <summary>
        /// Returns the previously registered object.
        /// </summary>
        /// <remarks>
        /// Call this method to deserialize a previously loaded object reference.
        /// </remarks>
        public object Get( Guid id );
    }

    /// <summary>
    /// <see cref="ILoader"/>, but can save over multiple frames.
    /// </summary>
    public interface IAsyncLoader : ILoader
    {
        /// <summary>
        /// The percentage (in [0..1]) of completion of the current action (0 = 0% completed, 1 = 100% completed).
        /// </summary>
        /// <remarks>
        /// Intended to be set by the serialization strategy, and not by the loader.
        /// </remarks>
        float CurrentActionPercentCompleted { get; set; }
        /// <summary>
        /// The percentage (in [0..1]) of completion of the entire loading process (0 = 0% completed, 1 = 100% completed).
        /// </summary>
        float TotalPercentCompleted { get; }
    }
}