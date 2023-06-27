using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Inherit from this to specify that your class handles assigning persistent data for itself.
    /// </summary>
    public interface IPersistent
    {
        /// <summary>
        /// Sets the persistent data after creating the object with default parameters.
        /// </summary>
        /// <param name="l">The loader. Can be used to read references, etc.</param>
        /// <param name="data">The serialized structure that contains the data. Identical to what is created by <see cref="GetData"/>.</param>
        void SetData( ILoader l, SerializedData data );

        /// <summary>
        /// Gets the persistent data from an object.
        /// </summary>
        /// <param name="s">The saver. Can be used to write references, etc.</param>
        /// <returns>The serialized structure that contains the data. Identical to what is read by <see cref="SetData"/>.</returns>
        SerializedData GetData( ISaver s );
    }
}