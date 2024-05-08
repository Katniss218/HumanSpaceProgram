using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// An abstract tree structure node representing arbitrary serialized data.
    /// </summary>
    /// <remarks>
    /// See also: <br/>
    /// - <see cref="SerializedObject"/> <br/>
    /// - <see cref="SerializedArray"/>
    /// </remarks>
    public abstract class SerializedData
    {
        /// <summary>
        /// Accesses a child element by its index (if applicable).
        /// </summary>
        public abstract SerializedData this[int index] { get; set; }

        /// <summary>
        /// Accesses a child element by its name (if applicable).
        /// </summary>
        public abstract SerializedData this[string name] { get; set; }

        /// <summary>
        /// Tries to access a child element by its name.
        /// </summary>
        /// <param name="name">The name of the child element to get.</param>
        /// <param name="value">The child element (if the returned value was true).</param>
        /// <returns>True if the child element exists.</returns>
        public abstract bool TryGetValue( string name, out SerializedData value );

        /// <summary>
        /// Tries to access a child element of a specified type by its name.
        /// </summary>
        /// <param name="name">The name of the child element to get.</param>
        /// <param name="value">The child element (if the returned value was true).</param>
        /// <returns>True if the child element exists and is of a specified type.</returns>
        public abstract bool TryGetValue<T>( string name, out T value ) where T : SerializedData;
    }
}