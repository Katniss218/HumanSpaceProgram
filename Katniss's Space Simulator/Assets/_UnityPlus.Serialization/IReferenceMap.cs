using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public interface IForwardReferenceMap
    {
        /// <summary>
        /// Tries to return a previously registered object for a specific ID.
        /// </summary>
        bool TryGetObj( Guid id, out object obj );

        /// <summary>
        /// Returns the previously registered object.
        /// </summary>
        /// <remarks>
        /// Should return <see cref="null"/> for invalid references.
        /// </remarks>
        object GetObj( Guid id );
        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        void SetObj( Guid id, object obj );
    }

    public interface IReverseReferenceMap
    {
        /// <summary>
        /// Tries to return the ID of a previously registered object.
        /// </summary>
        bool TryGetID( object obj, out Guid id );
        /// <summary>
        /// Returns the ID for the given object.
        /// </summary>
        /// <remarks>
        /// Should register the object under a random unique ID if the object is not yet registered.
        /// </remarks>
        Guid GetID( object obj );
        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        void SetID( object obj, Guid id );
    }
}