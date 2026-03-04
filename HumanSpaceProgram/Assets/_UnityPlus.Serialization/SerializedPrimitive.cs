using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// An arbitrary supported primitive value stored in the tree structure.
    /// </summary>
    [DebuggerDisplay( "{ToString()}" )]
    public sealed class SerializedPrimitive : SerializedData, IEquatable<SerializedPrimitive>
    {
        /// <summary>
        /// Stores the actual value of the primitive. The length is 24 bytes (16 + ref)
        /// </summary>
        [StructLayout( LayoutKind.Explicit )]
        internal struct Value
        {
            // Used to compare different primitives...
            [FieldOffset( 0 )] public Guid equalityValue;
            [FieldOffset( 16 )] public object equalityReference;

            // Used to store the actual value...
            // We overlap the fields, since the data container can only store a single value at a time.
            [FieldOffset( 0 )] public bool boolean;
            [FieldOffset( 0 )] public long int64;
            [FieldOffset( 0 )] public ulong uint64;
            [FieldOffset( 0 )] public double float64;
            [FieldOffset( 0 )] public decimal @decimal;
            [FieldOffset( 16 )] public string str; // Reference types can't be overlapped with value types :(, 16 bytes is the size of decimal.

            public string ToString( DataType type )
            {
                var inv = CultureInfo.InvariantCulture;

                return type switch
                {
                    DataType.Boolean =>
                        $"{(boolean ? "true" : "false")} (bool)",

                    DataType.Int64 =>
                        $"{int64.ToString( inv )} (i64)",

                    DataType.UInt64 =>
                        $"{uint64.ToString( inv )} (u64)",

                    DataType.Float64 =>
                        FormatDouble(),

                    DataType.Decimal =>
                        $"{@decimal.ToString( inv )} (dec)",

                    DataType.String =>
                        str == null
                            ? "null (str)"
                            : $"\"{Escape( str )}\" (str)",

                    _ => "invalid()"
                };
            }

            private string FormatDouble()
            {
                var inv = CultureInfo.InvariantCulture;

                if( double.IsNaN( float64 ) )
                    return "NaN (f64)";

                if( double.IsPositiveInfinity( float64 ) )
                    return "+Infinity (f64)";

                if( double.IsNegativeInfinity( float64 ) )
                    return "-Infinity (f64)";

                // "R" guarantees round-trip for double
                return $"{float64.ToString( "R", inv )} (f64)";
            }

            private static string Escape( string s )
            {
                var sb = new StringBuilder( s.Length + 8 );

                foreach( var c in s )
                {
                    switch( c )
                    {
                        case '\\': sb.Append( "\\\\" ); break;
                        case '\"': sb.Append( "\\\"" ); break;
                        case '\n': sb.Append( "\\n" ); break;
                        case '\r': sb.Append( "\\r" ); break;
                        case '\t': sb.Append( "\\t" ); break;
                        case '\0': sb.Append( "\\0" ); break;
                        default:
                            if( char.IsControl( c ) )
                                sb.Append( "\\u" ).Append( ((int)c).ToString( "X4" ) );
                            else
                                sb.Append( c );
                            break;
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Determines how to access the data inside the <see cref="Value"/>.
        /// </summary>
        internal enum DataType : byte
        {
            // 16-byte value types below (grouped for equality checking).

            /// <summary>
            /// The type information is missing or the primitive is malformed.
            /// </summary>
            Invalid = 0,
            /// <summary>
            /// Either 'true' or 'false'.
            /// </summary>
            Boolean,

            /// <summary>
            /// A 64-bit signed integer.
            /// </summary>
            Int64,
            /// <summary>
            /// A 64-bit unsigned integer.
            /// </summary>
            UInt64,

            /// <summary>
            /// A 64-bit IEEE754 floating-point.
            /// </summary>
            Float64,

            /// <summary>
            /// An 80-bit floating point with a base-10 exponent.
            /// </summary>
            Decimal,

            // reference-holding types below (grouped for equality checking).

            /// <summary>
            /// A sequence of characters of known length.
            /// </summary>
            String = 128
        }

        internal readonly Value _value;
        internal readonly DataType _type;

        private SerializedPrimitive( Value value, DataType type )
        {
            this._value = value;
            this._type = type;
        }

        public override SerializedData this[int index]
        {
            get => throw new NotSupportedException( $"Tried to invoke int indexer, which is not supported on {nameof( SerializedPrimitive )}." );
            set => throw new NotSupportedException( $"Tried to invoke int indexer, which is not supported on {nameof( SerializedPrimitive )}." );
        }

        public override SerializedData this[string name]
        {
            get => throw new NotSupportedException( $"Tried to invoke string indexer, which is not supported on {nameof( SerializedPrimitive )}." );
            set => throw new NotSupportedException( $"Tried to invoke string indexer, which is not supported on {nameof( SerializedPrimitive )}." );
        }

        public override bool TryGetValue( string name, out SerializedData value )
        {
            value = default;
            return false;
        }
        
        public override bool TryGetValue<T>( string name, out T value )
        {
            value = default;
            return false;
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedPrimitive val )
            {
                return this == val; // overriden equality op
            }

            return false;
        }

        public bool Equals( SerializedPrimitive other )
        {
            return this == other; // overriden equality op
        }

        public override int GetHashCode()
        {
            if( this._type >= DataType.String ) // reference types > 128
            {
                return this._value.equalityReference?.GetHashCode() ?? 0;
            }

            return this._value.equalityValue.GetHashCode();
        }

        public override string ToString()
        {
            return this._value.ToString( this._type );
        }

        public override string DumpToString()
        {
            return DumpToString( 0 );
        }

        internal override string DumpToString( int indentLevel )
        {
            string indent = string.Concat( Enumerable.Repeat( "- ", indentLevel ) );
            return $"{indent}{ToString()}";
        }

        public static implicit operator SerializedPrimitive( bool v ) => new SerializedPrimitive( new Value() { boolean = v }, DataType.Boolean );
        public static implicit operator SerializedPrimitive( sbyte v ) => new SerializedPrimitive( new Value() { int64 = v }, DataType.Int64 );
        public static implicit operator SerializedPrimitive( byte v ) => new SerializedPrimitive( new Value() { uint64 = v }, DataType.UInt64 );
        public static implicit operator SerializedPrimitive( short v ) => new SerializedPrimitive( new Value() { int64 = v }, DataType.Int64 );
        public static implicit operator SerializedPrimitive( ushort v ) => new SerializedPrimitive( new Value() { uint64 = v }, DataType.UInt64 );
        public static implicit operator SerializedPrimitive( int v ) => new SerializedPrimitive( new Value() { int64 = v }, DataType.Int64 );
        public static implicit operator SerializedPrimitive( uint v ) => new SerializedPrimitive( new Value() { uint64 = v }, DataType.UInt64 );
        public static implicit operator SerializedPrimitive( long v ) => new SerializedPrimitive( new Value() { int64 = v }, DataType.Int64 );
        public static implicit operator SerializedPrimitive( ulong v ) => new SerializedPrimitive( new Value() { uint64 = v }, DataType.UInt64 );
        public static implicit operator SerializedPrimitive( float v ) => new SerializedPrimitive( new Value() { float64 = v }, DataType.Float64 );
        public static implicit operator SerializedPrimitive( double v ) => new SerializedPrimitive( new Value() { float64 = v }, DataType.Float64 );
        public static implicit operator SerializedPrimitive( decimal v ) => new SerializedPrimitive( new Value() { @decimal = v }, DataType.Decimal );
        public static implicit operator SerializedPrimitive( string v ) => new SerializedPrimitive( new Value() { str = v }, DataType.String );

        public static implicit operator bool( SerializedPrimitive v ) => v._type switch
        {
            DataType.Boolean => v._value.boolean,
            _ => throw new InvalidOperationException( $"Can't convert to `bool` from `{v._type}`." ),
        };
        public static implicit operator sbyte( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (sbyte)v._value.int64,
            DataType.UInt64 => (sbyte)v._value.uint64,
            DataType.Float64 => (sbyte)v._value.float64,
            DataType.Decimal => (sbyte)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `sbyte` from `{v._type}`." ),
        };
        public static implicit operator byte( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (byte)v._value.int64,
            DataType.UInt64 => (byte)v._value.uint64,
            DataType.Float64 => (byte)v._value.float64,
            DataType.Decimal => (byte)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `byte` from `{v._type}`." ),
        };
        public static implicit operator short( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (short)v._value.int64,
            DataType.UInt64 => (short)v._value.uint64,
            DataType.Float64 => (short)v._value.float64,
            DataType.Decimal => (short)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `short` from `{v._type}`." ),
        };
        public static implicit operator ushort( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (ushort)v._value.int64,
            DataType.UInt64 => (ushort)v._value.uint64,
            DataType.Float64 => (ushort)v._value.float64,
            DataType.Decimal => (ushort)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `ushort` from `{v._type}`." ),
        };
        public static implicit operator int( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (int)v._value.int64,
            DataType.UInt64 => (int)v._value.uint64,
            DataType.Float64 => (int)v._value.float64,
            DataType.Decimal => (int)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `int` from `{v._type}`." ),
        };
        public static implicit operator uint( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (uint)v._value.int64,
            DataType.UInt64 => (uint)v._value.uint64,
            DataType.Float64 => (uint)v._value.float64,
            DataType.Decimal => (uint)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `uint` from `{v._type}`." ),
        };
        public static implicit operator long( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (long)v._value.int64,
            DataType.UInt64 => (long)v._value.uint64,
            DataType.Float64 => (long)v._value.float64,
            DataType.Decimal => (long)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `long` from `{v._type}`." ),
        };
        public static implicit operator ulong( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (ulong)v._value.int64,
            DataType.UInt64 => (ulong)v._value.uint64,
            DataType.Float64 => (ulong)v._value.float64,
            DataType.Decimal => (ulong)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `ulong` from `{v._type}`." ),
        };
        public static implicit operator float( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (float)v._value.int64,
            DataType.UInt64 => (float)v._value.uint64,
            DataType.Float64 => (float)v._value.float64,
            DataType.Decimal => (float)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `float` from `{v._type}`." ),
        };
        public static implicit operator double( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (double)v._value.int64,
            DataType.UInt64 => (double)v._value.uint64,
            DataType.Float64 => (double)v._value.float64,
            DataType.Decimal => (double)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `double` from `{v._type}`." ),
        };
        public static implicit operator decimal( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int64 => (decimal)v._value.int64,
            DataType.UInt64 => (decimal)v._value.uint64,
            DataType.Float64 => (decimal)v._value.float64,
            DataType.Decimal => (decimal)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `decimal` from `{v._type}`." ),
        };
        //                                                                   \/ string is the only type that can actually hold null. When deserializing, it can be initialized to null.
        public static implicit operator string( SerializedPrimitive v ) => v == null ? default : v._type switch
        {
            DataType.String => v._value.str,
            _ => throw new InvalidOperationException( $"Can't convert to `string` from `{v._type}`." ),
        };


        public static bool operator ==( SerializedPrimitive v1, SerializedPrimitive v2 )
        {
            if( v1 is null ) 
                return v2 is null;

            if( v2 is null )
                return v1 is null;

            if( v1._type >= DataType.String ) // reference types > 128
            {
                return v1._value.equalityReference.Equals( v2._value.equalityReference );
            }

            return v1._value.equalityValue == v2._value.equalityValue;
        }

        public static bool operator !=( SerializedPrimitive v1, SerializedPrimitive v2 )
        {
            return !(v1 == v2);
        }
    }
}