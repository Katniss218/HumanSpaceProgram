using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Inherit from this interface to specify that your class handles persisting its data by itself.
    /// </summary>
    public interface IPersistent
    {
        /// <summary>
        /// Gets the persistent data from an object.
        /// </summary>
        /// <remarks>
        /// This should return the data required to reconstruct the full internal state of the object.
        /// </remarks>
        /// <param name="s">The saver. Can be used to write references, etc.</param>
        /// <returns>The serialized structure that contains the data. Identical to what is read by <see cref="SetData"/>.</returns>
        SerializedData GetData( IReverseReferenceMap s );

        /// <summary>
        /// Sets the persistent data after creating the object with default parameters.
        /// </summary>
        /// <remarks>
        /// This should reconstruct the full internal state of the object from the given data. <br />
        /// *The data may be partial.*
        /// </remarks>
        /// <param name="l">The loader. Can be used to read references, etc.</param>
        /// <param name="data">The serialized structure that contains the data. Identical to what is created by <see cref="GetData"/>.</param>
        void SetData( IForwardReferenceMap l, SerializedData data );
    }
}