using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Json
{
    public class JsonStringWriter
    {
        StringBuilder _sb;
        SerializedData _data;

        public JsonStringWriter( SerializedData data, StringBuilder sb )
        {
            this._data = data;
            this._sb = sb;
        }

        public void Write()
        {
            WriteJson( _data );
        }

        void WriteJson( SerializedData data )
        {
            if( data is SerializedObject o )
                WriteJson( o );
            else if( data is SerializedArray a )
                WriteJson( a );
            else if( data is SerializedPrimitive v )
                WriteJson( v );
        }

        void WriteJson( SerializedObject obj )
        {
            _sb.Append( '{' );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    _sb.Append( ',' );
                }
                else
                {
                    seen = true;
                }

                var str = $"\"{child.Key}\":";

                _sb.Append( str );

                WriteJson( child.Value );
            }

            _sb.Append( '}' );
        }

        void WriteJson( SerializedArray obj )
        {
            _sb.Append( '[' );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    _sb.Append( ',' );
                }
                else
                {
                    seen = true;
                }
                WriteJson( child );
            }

            _sb.Append( ']' );
        }

        void WriteJson( SerializedPrimitive value )
        {
            if( value == null )
            {
#warning TODO - move all of these constants and stuff to a separate class.
                _sb.Append( "null" );
                return;
            }

            string s = null;
            switch( value._type )
            {
                case SerializedPrimitive.DataType.Boolean:
                    s = value._value.boolean ? "true" : "false"; break;
                case SerializedPrimitive.DataType.Int:
                    s = value._value.@int.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.UInt:
                    s = value._value.@uint.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.Float:
                    s = value._value.@float.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.Decimal:
                    s = value._value.@decimal.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.String:
                    WriteString( value._value.str ); return;
            }

            _sb.Append( s );
        }

        void WriteString( string sIn )
        {
            if( sIn == null )
            {
                _sb.Append( "null" );
                return;
            }

            _sb.Append( '\"' );

            int i = 0;
            int start = 0;
            foreach( var c in sIn )
            {
                if( c is '\\' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\\\" );
                }
                else if( c is '\"' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\\"" );
                }
                else if( c is '\n' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\n" );
                }
                else if( c is '\r' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\r" );
                }
                else if( c is '\t' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\t" );
                }
                else if( c is '\b' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\b" );
                }
                else if( c is '\f' )
                {
                    _sb.Append( sIn[start..i] );
                    _sb.Append( "\\f" );
                }
                else
                {
                    i++;
                    continue;
                }

                i++;
                start = i;
            }

            if( i - start > 0 ) // write last (or the only if no escaping) part 
            {
                _sb.Append( sIn[start..i] );
            }

            _sb.Append( '\"' );
        }
    }
}