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
        /// Loading is split into 2 stages.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// Not saving.
            /// </summary>
            Idle = 0,

            /// <summary>
            /// 1. Creation of referencable objects. <br />
            ///    These objects will have default parameters, can can be created by a factory, or a number of other methods. <br />
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
        /// Call this method when loading an object that might be referenced.
        /// </remarks>
        void SetID( object obj, Guid id );

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
        float CurrentActionPercentCompleted { get; set; }
        float TotalPercentCompleted { get; }
    }
}