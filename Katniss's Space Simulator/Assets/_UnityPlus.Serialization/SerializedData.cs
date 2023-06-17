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
    /// A tree-like structure representing an arbitrary serialized data structure.
    /// </summary>
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

        public static implicit operator SerializedData( bool v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( sbyte v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( byte v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( short v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( ushort v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( int v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( uint v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( long v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( ulong v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( float v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( double v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( decimal v ) => (SerializedPrimitive)v;
        public static implicit operator SerializedData( string v ) => (SerializedPrimitive)v;

        public static implicit operator bool( SerializedData v ) => (bool)(SerializedPrimitive)v;
        public static implicit operator sbyte( SerializedData v ) => (sbyte)(SerializedPrimitive)v;
        public static implicit operator byte( SerializedData v ) => (byte)(SerializedPrimitive)v;
        public static implicit operator short( SerializedData v ) => (short)(SerializedPrimitive)v;
        public static implicit operator ushort( SerializedData v ) => (ushort)(SerializedPrimitive)v;
        public static implicit operator int( SerializedData v ) => (int)(SerializedPrimitive)v;
        public static implicit operator uint( SerializedData v ) => (uint)(SerializedPrimitive)v;
        public static implicit operator long( SerializedData v ) => (long)(SerializedPrimitive)v;
        public static implicit operator ulong( SerializedData v ) => (ulong)(SerializedPrimitive)v;
        public static implicit operator float( SerializedData v ) => (float)(SerializedPrimitive)v;
        public static implicit operator double( SerializedData v ) => (double)(SerializedPrimitive)v;
        public static implicit operator decimal( SerializedData v ) => (decimal)(SerializedPrimitive)v;
        public static implicit operator string( SerializedData v ) => (string)(SerializedPrimitive)v;
    }
}