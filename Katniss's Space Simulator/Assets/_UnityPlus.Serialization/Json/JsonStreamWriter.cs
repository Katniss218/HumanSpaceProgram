using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Json
{
    public class JsonStreamWriter
    {
        static readonly Encoding enc = Encoding.UTF8;

        static readonly byte[] openObject = enc.GetBytes( "{" );
        static readonly byte[] closeObject = enc.GetBytes( "}" );
        static readonly byte[] openArray = enc.GetBytes( "[" );
        static readonly byte[] closeArray = enc.GetBytes( "]" );
        static readonly byte[] comma = enc.GetBytes( "," );

        Stream _stream;
        SerializedData _data;

        public JsonStreamWriter( SerializedData data, Stream stream )
        {
            this._data = data;
            this._stream = stream;
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
            _stream.Write( openObject, 0, 1 );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    _stream.Write( comma, 0, 1 );
                }
                else
                {
                    seen = true;
                }

                var str = $"\"{child.Key}\":";

                _stream.Write( enc.GetBytes( str ), 0, str.Length );

                WriteJson( child.Value );
            }

            _stream.Write( closeObject, 0, 1 );
        }

        void WriteJson( SerializedArray obj )
        {
            _stream.Write( openArray, 0, 1 );

            bool seen = false;
            foreach( var child in obj )
            {
                if( seen )
                {
                    _stream.Write( comma, 0, 1 );
                }
                else
                {
                    seen = true;
                }
                WriteJson( child );
            }

            _stream.Write( closeArray, 0, 1 );
        }

        void WriteJson( SerializedPrimitive value )
        {
            if( value == null )
            {
                _stream.Write( enc.GetBytes( "null" ), 0, "null".Length );
                return;
            }

            string s = null;
            switch( value._type )
            {
                case SerializedPrimitive.DataType.Boolean:
                    s = value._value.boolean ? "true" : "false"; break;
                case SerializedPrimitive.DataType.Int64:
                    s = value._value.int64.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.UInt64:
                    s = value._value.uint64.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.Float64:
                    s = value._value.float64.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.Decimal:
                    s = value._value.@decimal.ToString( CultureInfo.InvariantCulture ); break;
                case SerializedPrimitive.DataType.String:
                    WriteString(value._value.str); return;
            }

            _stream.Write( enc.GetBytes( s ), 0, s.Length );
        }

        static readonly byte[] quote = enc.GetBytes( "\"" );
        static readonly byte[] escapedBackslash = enc.GetBytes( "\\\\" );
        static readonly byte[] escapedQuote = enc.GetBytes( "\\\"" );
        static readonly byte[] escapedNewLine = enc.GetBytes( "\\n" );
        static readonly byte[] escapedR = enc.GetBytes( "\\r" );
        static readonly byte[] escapedTab = enc.GetBytes( "\\t" );
        static readonly byte[] escapedB = enc.GetBytes( "\\b" );
        static readonly byte[] escapedF = enc.GetBytes( "\\f" );

        void WriteString( string sIn )
        {
            _stream.Write( quote, 0, quote.Length );

            int i = 0;
            int start = 0;
            foreach( var c in sIn )
            {
                if( c is '\\' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedBackslash, 0, escapedBackslash.Length );
                }
                else if( c is '\"' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedQuote, 0, escapedQuote.Length );
                }
                else if( c is '\n' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedNewLine, 0, escapedNewLine.Length );
                }
                else if( c is '\r' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedR, 0, escapedR.Length );
                }
                else if( c is '\t' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedTab, 0, escapedTab.Length );
                }
                else if( c is '\b' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedB, 0, escapedB.Length );
                }
                else if( c is '\f' )
                {
                    byte[] b = enc.GetBytes( sIn[start..i] );
                    _stream.Write( b, 0, b.Length );
                    _stream.Write( escapedF, 0, escapedF.Length );
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
                byte[] b = enc.GetBytes( sIn[start..i] );
                _stream.Write( b, 0, b.Length );
            }

            _stream.Write( quote, 0, quote.Length );
        }
    }
}
