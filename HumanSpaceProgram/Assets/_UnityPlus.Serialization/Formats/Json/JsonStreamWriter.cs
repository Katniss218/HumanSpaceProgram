
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityPlus.Serialization.Json
{
    public class JsonStreamWriter
    {
        private readonly StreamWriter _writer;
        private readonly SerializedData _root;

        private struct WriteState
        {
            public IEnumerator<SerializedData> arrayEnumerator;
            public IEnumerator<KeyValuePair<string, SerializedData>> objectEnumerator;
            public bool first;
            public bool isObject;

            public WriteState( IEnumerator<SerializedData> en )
            {
                arrayEnumerator = en;
                objectEnumerator = null;
                first = true;
                isObject = false;
            }

            public WriteState( IEnumerator<KeyValuePair<string, SerializedData>> en )
            {
                arrayEnumerator = null;
                objectEnumerator = en;
                first = true;
                isObject = true;
            }
        }

        public JsonStreamWriter( SerializedData data, Stream stream )
        {
            _root = data;
            _writer = new StreamWriter( stream, new UTF8Encoding( false ), 4096 );
        }

        public void Write()
        {
            if( _root == null )
            {
                _writer.Write( "null" );
                _writer.Flush();
                return;
            }

            Stack<WriteState> stack = new Stack<WriteState>();

            if( PushValue( _root, stack ) )
            {
                while( stack.Count > 0 )
                {
                    var state = stack.Pop();

                    bool hasMore;
                    if( state.isObject )
                        hasMore = state.objectEnumerator.MoveNext();
                    else
                        hasMore = state.arrayEnumerator.MoveNext();

                    if( !hasMore )
                    {
                        if( state.isObject ) _writer.Write( '}' );
                        else _writer.Write( ']' );
                        continue;
                    }

                    if( !state.first )
                    {
                        _writer.Write( ',' );
                    }
                    state.first = false;
                    stack.Push( state );

                    SerializedData currentData;
                    if( state.isObject )
                    {
                        var kvp = state.objectEnumerator.Current;
                        JsonCommon.WriteEscapedString( kvp.Key, _writer );
                        _writer.Write( ':' );
                        currentData = kvp.Value;
                    }
                    else
                    {
                        currentData = state.arrayEnumerator.Current;
                    }

                    PushValue( currentData, stack );
                }
            }

            _writer.Flush();
        }

        private bool PushValue( SerializedData data, Stack<WriteState> stack )
        {
            if( data == null )
            {
                _writer.Write( "null" );
                return false;
            }

            if( data is SerializedPrimitive prim )
            {
                WritePrimitive( prim );
                return false;
            }

            if( data is SerializedObject obj )
            {
                _writer.Write( '{' );
                stack.Push( new WriteState( obj.GetEnumerator() ) );
                return true;
            }

            if( data is SerializedArray arr )
            {
                _writer.Write( '[' );
                stack.Push( new WriteState( arr.GetEnumerator() ) );
                return true;
            }

            return false;
        }

        private void WritePrimitive( SerializedPrimitive p )
        {
            switch( p._type )
            {
                case SerializedPrimitive.DataType.Boolean:
                    _writer.Write( p._value.boolean ? "true" : "false" );
                    break;
                case SerializedPrimitive.DataType.Int64:
                    _writer.Write( p._value.int64.ToString( CultureInfo.InvariantCulture ) );
                    break;
                case SerializedPrimitive.DataType.UInt64:
                    _writer.Write( p._value.uint64.ToString( CultureInfo.InvariantCulture ) );
                    break;
                case SerializedPrimitive.DataType.Float64:
                    _writer.Write( p._value.float64.ToString( CultureInfo.InvariantCulture ) );
                    break;
                case SerializedPrimitive.DataType.Decimal:
                    _writer.Write( p._value.@decimal.ToString( CultureInfo.InvariantCulture ) );
                    break;
                case SerializedPrimitive.DataType.String:
                    JsonCommon.WriteEscapedString( p._value.str, _writer );
                    break;
                default:
                    _writer.Write( "null" );
                    break;
            }
        }
    }
}
