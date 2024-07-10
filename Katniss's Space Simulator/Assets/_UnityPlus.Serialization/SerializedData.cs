
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