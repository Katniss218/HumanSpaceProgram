using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents a structure that has the ability to map objects to IDs.
    /// </summary>
    public interface IReverseReferenceMap
    {
        /// <summary>
        /// Gets all entries in the reference map as a collection of tuples.
        /// </summary>
        IEnumerable<(Guid id, object val)> GetAll();

        /// <summary>
        /// Adds a collection of entries to the reference map.
        /// </summary>
        void AddAll( IEnumerable<(Guid id, object val)> data );

        /// <summary>
        /// Tries to return the ID of a previously registered object.
        /// </summary>
        /// <remarks>
        /// Should always return false for <see cref="null"/> and for references that are not registered.
        /// </remarks>
        bool TryGetID( object obj, out Guid id );

        /// <summary>
        /// Returns the ID for the given object.
        /// </summary>
        /// <remarks>
        /// Should always return <see cref="Guid.Empty"/> for <see cref="null"/>. <br />
        /// Should register the <paramref name="obj"/> under a new <see cref="Guid"/> if not yet registered.
        /// </remarks>
        Guid GetID( object obj );

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        void SetID( object obj, Guid id );
    }
}