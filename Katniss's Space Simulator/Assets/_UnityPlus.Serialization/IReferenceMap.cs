using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// An arbitrary structure that has the ability to map IDs to Objects.
    /// </summary>
    public interface IForwardReferenceMap
    {
        /// <summary>
        /// Gets all entries in the reference map as a list of tuples.
        /// </summary>
        IEnumerable<(Guid id, object val)> GetAll();

        /// <summary>
        /// Adds entries to the reference map.
        /// </summary>
        void AddAll( IEnumerable<(Guid id, object val)> data );

        /// <summary>
        /// Tries to return a previously registered object for a specific ID.
        /// </summary>
        /// <remarks>
        /// Should always return false for references that are not registered.
        /// </remarks>
        bool TryGetObj( Guid id, out object obj );

        /// <summary>
        /// Returns the previously registered object.
        /// </summary>
        /// <remarks>
        /// Should always return <see cref="null"/> for <see cref="Guid.Empty"/>. <br />
        /// </remarks>
        object GetObj( Guid id );

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        void SetObj( Guid id, object obj );
    }

    /// <summary>
    /// An arbitrary structure that has the ability to map Objects to IDs.
    /// </summary>
    public interface IReverseReferenceMap
    {
        /// <summary>
        /// Gets all entries in the reference map as a list of tuples.
        /// </summary>
        IEnumerable<(Guid id, object val)> GetAll();

        /// <summary>
        /// Adds entries to the reference map.
        /// </summary>
        void AddAll( IEnumerable<(Guid id, object val)> data );

        /// <summary>
        /// Tries to return the ID of a previously registered object.
        /// </summary>
        /// <remarks>
        /// Should always return false for <see cref="null"/>.
        /// </remarks>
        bool TryGetID( object obj, out Guid id );

        /// <summary>
        /// Returns the ID for the given object.
        /// </summary>
        /// <remarks>
        /// Should always return <see cref="Guid.Empty"/> for <see cref="null"/>. <br />
        /// Should register the <paramref name="obj"/> under a random <see cref="Guid"/> if not yet registered.
        /// </remarks>
        Guid GetID( object obj );

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        void SetID( object obj, Guid id );
    }
}