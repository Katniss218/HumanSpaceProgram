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
        /// Gets all entries in the reference map as a collection of tuples.
        /// </summary>
        IEnumerable<(Guid id, object val)> GetAll();

        /// <summary>
        /// Adds a collection of entries to the reference map.
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
}