using System;

namespace UnityPlus.Serialization
{
    [Flags]
    public enum ObjectHeaderStyle : byte
    {
        /// <summary>
        /// Don't add a type header.
        /// </summary>
        None = 0,
        /// <summary>
        /// Add a type header and the '$type' field.
        /// </summary>
        TypeField = 1,
        /// <summary>
        /// Add the type header and the '$id' field.
        /// </summary>
        IDField = 2
    }
}