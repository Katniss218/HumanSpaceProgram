using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// An arbitrary supported primitive value stored in the tree structure.
    /// </summary>
    public sealed class SerializedPrimitive : SerializedData
    // any value:
    // - boolean `true`
    // - number `123.456`
    // - string `"string"`
    // - object `{ serializedObject }`
    // - array `[ serializedArray ]`
    {
        [StructLayout( LayoutKind.Explicit )]
        internal struct DataContainer
        {
            [FieldOffset( 0 )] public Guid equalityValue;
            [FieldOffset( 16 )] public object equalityReference;

            [FieldOffset( 0 )] public bool boolean;
            [FieldOffset( 0 )] public long @int;
            [FieldOffset( 0 )] public ulong @uint;
            [FieldOffset( 0 )] public double @float;
            [FieldOffset( 0 )] public decimal @decimal;
            [FieldOffset( 16 )] public string str; // decimal is a big boi, 16 bytes offset.
        }

        internal enum DataType : byte
        {
            // 16-byte value types.
            Invalid = 0,

            Boolean,

            Int,
            UInt,

            Float,
            Decimal,

            // reference-holding types below, starting at 128.
            String = 128
        }

        internal readonly DataContainer _value;
        internal readonly DataType _type;

        SerializedPrimitive( DataContainer value, DataType type )
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
            throw new NotSupportedException( $"Tried to invoke {nameof( TryGetValue )}, which is not supported on {nameof( SerializedPrimitive )}." );
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedPrimitive val )
            {
                return this == val; // overriden equality op
            }

            return false;
        }

        public override int GetHashCode()
        {
            if( this._type >= DataType.String ) // reference types > 128
            {
                return this._value.equalityReference.GetHashCode();
            }

            return this._value.equalityValue.GetHashCode();
        }


        public static implicit operator SerializedPrimitive( bool v ) => new SerializedPrimitive( new DataContainer() { boolean = v }, DataType.Boolean );
        public static implicit operator SerializedPrimitive( sbyte v ) => new SerializedPrimitive( new DataContainer() { @int = v }, DataType.Int );
        public static implicit operator SerializedPrimitive( byte v ) => new SerializedPrimitive( new DataContainer() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedPrimitive( short v ) => new SerializedPrimitive( new DataContainer() { @int = v }, DataType.Int );
        public static implicit operator SerializedPrimitive( ushort v ) => new SerializedPrimitive( new DataContainer() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedPrimitive( int v ) => new SerializedPrimitive( new DataContainer() { @int = v }, DataType.Int );
        public static implicit operator SerializedPrimitive( uint v ) => new SerializedPrimitive( new DataContainer() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedPrimitive( long v ) => new SerializedPrimitive( new DataContainer() { @int = v }, DataType.Int );
        public static implicit operator SerializedPrimitive( ulong v ) => new SerializedPrimitive( new DataContainer() { @uint = v }, DataType.UInt );
        public static implicit operator SerializedPrimitive( float v ) => new SerializedPrimitive( new DataContainer() { @float = v }, DataType.Float );
        public static implicit operator SerializedPrimitive( double v ) => new SerializedPrimitive( new DataContainer() { @float = v }, DataType.Float );
        public static implicit operator SerializedPrimitive( decimal v ) => new SerializedPrimitive( new DataContainer() { @decimal = v }, DataType.Decimal );
        public static implicit operator SerializedPrimitive( string v ) => new SerializedPrimitive( new DataContainer() { str = v }, DataType.String );

        public static implicit operator bool( SerializedPrimitive v ) => v._type switch
        {
            DataType.Boolean => v._value.boolean,
            _ => throw new InvalidOperationException( $"Can't convert to `bool` from `{v._type}`." ),
        };
        public static implicit operator sbyte( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (sbyte)v._value.@int,
            DataType.UInt => (sbyte)v._value.@uint,
            DataType.Float => (sbyte)v._value.@float,
            DataType.Decimal => (sbyte)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `sbyte` from `{v._type}`." ),
        };
        public static implicit operator byte( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (byte)v._value.@int,
            DataType.UInt => (byte)v._value.@uint,
            DataType.Float => (byte)v._value.@float,
            DataType.Decimal => (byte)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `byte` from `{v._type}`." ),
        };
        public static implicit operator short( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (short)v._value.@int,
            DataType.UInt => (short)v._value.@uint,
            DataType.Float => (short)v._value.@float,
            DataType.Decimal => (short)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `short` from `{v._type}`." ),
        };
        public static implicit operator ushort( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (ushort)v._value.@int,
            DataType.UInt => (ushort)v._value.@uint,
            DataType.Float => (ushort)v._value.@float,
            DataType.Decimal => (ushort)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `ushort` from `{v._type}`." ),
        };
        public static implicit operator int( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (int)v._value.@int,
            DataType.UInt => (int)v._value.@uint,
            DataType.Float => (int)v._value.@float,
            DataType.Decimal => (int)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `int` from `{v._type}`." ),
        };
        public static implicit operator uint( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (uint)v._value.@int,
            DataType.UInt => (uint)v._value.@uint,
            DataType.Float => (uint)v._value.@float,
            DataType.Decimal => (uint)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `uint` from `{v._type}`." ),
        };
        public static implicit operator long( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (long)v._value.@int,
            DataType.UInt => (long)v._value.@uint,
            DataType.Float => (long)v._value.@float,
            DataType.Decimal => (long)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `long` from `{v._type}`." ),
        };
        public static implicit operator ulong( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (ulong)v._value.@int,
            DataType.UInt => (ulong)v._value.@uint,
            DataType.Float => (ulong)v._value.@float,
            DataType.Decimal => (ulong)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `ulong` from `{v._type}`." ),
        };
        public static implicit operator float( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (float)v._value.@int,
            DataType.UInt => (float)v._value.@uint,
            DataType.Float => (float)v._value.@float,
            DataType.Decimal => (float)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `float` from `{v._type}`." ),
        };
        public static implicit operator double( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (double)v._value.@int,
            DataType.UInt => (double)v._value.@uint,
            DataType.Float => (double)v._value.@float,
            DataType.Decimal => (double)v._value.@decimal,
            _ => throw new InvalidOperationException( $"Can't convert to `double` from `{v._type}`." ),
        };
        public static implicit operator decimal( SerializedPrimitive v ) => v._type switch
        {
            DataType.Int => (decimal)v._value.@int,
            DataType.UInt => (decimal)v._value.@uint,
            DataType.Float => (decimal)v._value.@float,
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
            if( (object)v1 == null ) return (object)v2 == null;
            if( (object)v2 == null ) return (object)v1 == null;

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