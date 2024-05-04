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

        public abstract bool TryGetValue( string name, out SerializedData value );
        public abstract bool TryGetValue<T>( string name, out T value ) where T : SerializedData;
    }
}