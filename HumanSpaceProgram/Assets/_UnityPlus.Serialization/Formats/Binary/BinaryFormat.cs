using System;
using System.IO;
using System.Text;

namespace UnityPlus.Serialization.Formats
{
    /// <summary>
    /// Binary format implementation.
    /// Uses Type-Tag prefixing and Count-prefixed containers for efficient storage.
    /// </summary>
    public class BinaryFormat : ISerializationFormat
    {
        public static readonly BinaryFormat Instance = new BinaryFormat();

        private const byte TYPE_NULL = 0;
        private const byte TYPE_BOOL_TRUE = 1;
        private const byte TYPE_BOOL_FALSE = 2;
        private const byte TYPE_INT64 = 3;
        private const byte TYPE_UINT64 = 4;
        private const byte TYPE_DOUBLE = 5;
        private const byte TYPE_STRING = 6;
        private const byte TYPE_OBJECT = 7;
        private const byte TYPE_ARRAY = 8;
        private const byte TYPE_DECIMAL = 9;

        public SerializedData Read( Stream stream )
        {
            // LeaveOpen = true so we don't close the stream provided by the handler
            using( var reader = new BinaryReader( stream, Encoding.UTF8, true ) )
            {
                return ReadValue( reader );
            }
        }

        public void Write( Stream stream, SerializedData data )
        {
            using( var writer = new BinaryWriter( stream, Encoding.UTF8, true ) )
            {
                WriteValue( writer, data );
            }
        }

        private SerializedData ReadValue( BinaryReader reader )
        {
            // Check for end of stream before reading type byte if possible, though ReadByte throws generic EOS exception usually.
            // In this recursive context, we expect data to be valid.

            byte type = reader.ReadByte();
            switch( type )
            {
                case TYPE_NULL: return null;
                case TYPE_BOOL_TRUE: return true;
                case TYPE_BOOL_FALSE: return false;
                case TYPE_INT64: return reader.ReadInt64();
                case TYPE_UINT64: return reader.ReadUInt64();
                case TYPE_DOUBLE: return reader.ReadDouble();
                case TYPE_DECIMAL: return reader.ReadDecimal();
                case TYPE_STRING: return reader.ReadString();

                case TYPE_OBJECT:
                    int objCount = reader.ReadInt32();
                    var obj = new SerializedObject( objCount );
                    for( int i = 0; i < objCount; i++ )
                    {
                        string key = reader.ReadString();
                        SerializedData val = ReadValue( reader );
                        obj[key] = val;
                    }
                    return obj;

                case TYPE_ARRAY:
                    int arrCount = reader.ReadInt32();
                    var arr = new SerializedArray( arrCount );
                    for( int i = 0; i < arrCount; i++ )
                    {
                        arr.Add( ReadValue( reader ) );
                    }
                    return arr;

                default: throw new FormatException( $"Unknown Binary Tag: {type}" );
            }
        }

        private void WriteValue( BinaryWriter writer, SerializedData data )
        {
            if( data == null )
            {
                writer.Write( TYPE_NULL );
                return;
            }

            if( data is SerializedPrimitive prim )
            {
                switch( prim._type )
                {
                    case SerializedPrimitive.DataType.Boolean:
                        writer.Write( prim._value.boolean ? TYPE_BOOL_TRUE : TYPE_BOOL_FALSE );
                        break;
                    case SerializedPrimitive.DataType.Int64:
                        writer.Write( TYPE_INT64 );
                        writer.Write( prim._value.int64 );
                        break;
                    case SerializedPrimitive.DataType.UInt64:
                        writer.Write( TYPE_UINT64 );
                        writer.Write( prim._value.uint64 );
                        break;
                    case SerializedPrimitive.DataType.Float64:
                        writer.Write( TYPE_DOUBLE );
                        writer.Write( prim._value.float64 );
                        break;
                    case SerializedPrimitive.DataType.Decimal:
                        writer.Write( TYPE_DECIMAL );
                        writer.Write( prim._value.@decimal );
                        break;
                    case SerializedPrimitive.DataType.String:
                        writer.Write( TYPE_STRING );
                        writer.Write( prim._value.str );
                        break;
                    default:
                        // Fallback
                        writer.Write( TYPE_STRING );
                        writer.Write( prim.ToString() );
                        break;
                }
            }
            else if( data is SerializedObject obj )
            {
                writer.Write( TYPE_OBJECT );
                writer.Write( obj.Count );
                foreach( var kvp in obj )
                {
                    writer.Write( kvp.Key );
                    WriteValue( writer, kvp.Value );
                }
            }
            else if( data is SerializedArray arr )
            {
                writer.Write( TYPE_ARRAY );
                writer.Write( arr.Count );
                foreach( var item in arr )
                {
                    WriteValue( writer, item );
                }
            }
        }
    }
}