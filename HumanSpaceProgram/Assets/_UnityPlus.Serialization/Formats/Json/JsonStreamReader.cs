using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityPlus.Serialization.Json
{
    public class JsonStreamReader
    {
        private readonly TextReader _reader;
        private readonly StringBuilder _sb = new StringBuilder();
        private int _recursionDepth;
        private const int MaxDepth = 512;

        public JsonStreamReader( Stream stream )
        {
            _reader = new StreamReader( stream, Encoding.UTF8, true, 1024 );
        }

        public JsonStreamReader( TextReader reader )
        {
            _reader = reader;
        }

        public SerializedData Read()
        {
            _recursionDepth = 0;
            SkipWhitespace();
            int peek = _reader.Peek();
            if( peek == -1 ) return null;

            return ParseValue();
        }

        private SerializedData ParseValue()
        {
            if( _recursionDepth > MaxDepth ) throw new FormatException( "JSON recursion depth limit exceeded." );

            SkipWhitespace();
            int c = _reader.Peek();

            switch( c )
            {
                case '{':
                    _recursionDepth++;
                    var obj = ParseObject();
                    _recursionDepth--;
                    return obj;
                case '[':
                    _recursionDepth++;
                    var arr = ParseArray();
                    _recursionDepth--;
                    return arr;
                case '"': return ParseString();
                case 't': MatchLiteral( "true" ); return true;
                case 'f': MatchLiteral( "false" ); return false;
                case 'n': MatchLiteral( "null" ); return null;
                default:
                    if( c == '-' || char.IsDigit( (char)c ) ) return ParseNumber();
                    throw new FormatException( $"Invalid JSON token: {(char)c}" );
            }
        }

        private SerializedObject ParseObject()
        {
            _reader.Read(); // Consume '{'
            var obj = new SerializedObject();
            bool first = true;

            while( true )
            {
                SkipWhitespace();
                int c = _reader.Peek();
                if( c == '}' )
                {
                    _reader.Read();
                    break;
                }

                if( !first )
                {
                    if( c != ',' ) throw new FormatException( "Expected ','" );
                    _reader.Read();
                    SkipWhitespace();
                }
                first = false;

                string key = (string)(SerializedPrimitive)ParseString();

                SkipWhitespace();
                if( _reader.Read() != ':' ) throw new FormatException( "Expected ':'" );

                SerializedData value = ParseValue();
                obj[key] = value;
            }
            return obj;
        }

        private SerializedArray ParseArray()
        {
            _reader.Read(); // Consume '['
            var arr = new SerializedArray();
            bool first = true;

            while( true )
            {
                SkipWhitespace();
                int c = _reader.Peek();
                if( c == ']' )
                {
                    _reader.Read();
                    break;
                }

                if( !first )
                {
                    if( c != ',' ) throw new FormatException( "Expected ','" );
                    _reader.Read();
                    SkipWhitespace();
                }
                first = false;

                SerializedData value = ParseValue();
                arr.Add( value );
            }
            return arr;
        }

        private SerializedPrimitive ParseString()
        {
            if( _reader.Read() != '"' ) throw new FormatException( "Expected string start" );

            _sb.Clear();

            while( true )
            {
                int c = _reader.Read();
                if( c == -1 ) throw new FormatException( "Unexpected end of stream in string" );

                if( c == '\\' )
                {
                    int escaped = _reader.Read();
                    if( escaped == -1 ) throw new FormatException( "Unexpected end of stream in escape sequence" );

                    switch( escaped )
                    {
                        case '"': _sb.Append( '"' ); break;
                        case '\\': _sb.Append( '\\' ); break;
                        case '/': _sb.Append( '/' ); break;
                        case 'b': _sb.Append( '\b' ); break;
                        case 'f': _sb.Append( '\f' ); break;
                        case 'n': _sb.Append( '\n' ); break;
                        case 'r': _sb.Append( '\r' ); break;
                        case 't': _sb.Append( '\t' ); break;
                        case 'u':
                            char[] hex = new char[4];
                            if( _reader.Read( hex, 0, 4 ) < 4 ) throw new FormatException( "Invalid unicode escape" );

                            int val = 0;
                            for( int i = 0; i < 4; i++ )
                            {
                                char h = hex[i];
                                val <<= 4;
                                if( h >= '0' && h <= '9' ) val += h - '0';
                                else if( h >= 'a' && h <= 'f' ) val += h - 'a' + 10;
                                else if( h >= 'A' && h <= 'F' ) val += h - 'A' + 10;
                                else throw new FormatException( "Invalid hex digit in unicode escape." );
                            }
                            _sb.Append( (char)val );
                            break;
                        default:
                            throw new FormatException( $"Invalid escape character: \\{(char)escaped}" );
                    }
                }
                else if( c == '"' )
                {
                    break;
                }
                else
                {
                    _sb.Append( (char)c );
                }
            }

            return (SerializedPrimitive)_sb.ToString();
        }

        private SerializedPrimitive ParseNumber()
        {
            _sb.Clear();
            bool isFloat = false;

            // 1. Minus
            int peek = _reader.Peek();
            if( peek == '-' )
            {
                _sb.Append( (char)_reader.Read() );
                peek = _reader.Peek();
                if( peek == -1 || !char.IsDigit( (char)peek ) ) throw new FormatException( "Invalid number: digit expected after minus" );
            }

            // 2. Integer
            peek = _reader.Peek();
            if( peek == '0' )
            {
                _sb.Append( (char)_reader.Read() );
                peek = _reader.Peek();
                if( peek != -1 && char.IsDigit( (char)peek ) ) throw new FormatException( "Invalid number: leading zero" );
            }
            else if( char.IsDigit( (char)peek ) )
            {
                _sb.Append( (char)_reader.Read() );
                while( true )
                {
                    peek = _reader.Peek();
                    if( peek != -1 && char.IsDigit( (char)peek ) ) _sb.Append( (char)_reader.Read() );
                    else break;
                }
            }
            else
            {
                throw new FormatException( "Invalid number: expected digit" );
            }

            // 3. Fraction
            peek = _reader.Peek();
            if( peek == '.' )
            {
                isFloat = true;
                _sb.Append( (char)_reader.Read() );

                peek = _reader.Peek();
                if( peek == -1 || !char.IsDigit( (char)peek ) ) throw new FormatException( "Invalid number: expected digit after decimal" );

                while( true )
                {
                    peek = _reader.Peek();
                    if( peek != -1 && char.IsDigit( (char)peek ) ) _sb.Append( (char)_reader.Read() );
                    else break;
                }
            }

            // 4. Exponent
            peek = _reader.Peek();
            if( peek == 'e' || peek == 'E' )
            {
                isFloat = true;
                _sb.Append( (char)_reader.Read() );

                peek = _reader.Peek();
                if( peek == '+' || peek == '-' ) _sb.Append( (char)_reader.Read() );

                peek = _reader.Peek();
                if( peek == -1 || !char.IsDigit( (char)peek ) ) throw new FormatException( "Invalid number: expected digit after exponent" );

                while( true )
                {
                    peek = _reader.Peek();
                    if( peek != -1 && char.IsDigit( (char)peek ) ) _sb.Append( (char)_reader.Read() );
                    else break;
                }
            }

            string numStr = _sb.ToString();
            if( isFloat )
            {
                if( double.TryParse( numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double d ) )
                    return (SerializedPrimitive)d;
            }
            else
            {
                if( long.TryParse( numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l ) )
                    return (SerializedPrimitive)l;
            }
            throw new FormatException( $"Could not parse number: {numStr}" );
        }

        private void MatchLiteral( string literal )
        {
            foreach( char c in literal )
            {
                if( _reader.Read() != c ) throw new FormatException( $"Expected literal '{literal}'" );
            }
        }

        private void SkipWhitespace()
        {
            while( true )
            {
                int c = _reader.Peek();
                if( c == -1 ) return;
                if( !char.IsWhiteSpace( (char)c ) ) return;
                _reader.Read();
            }
        }
    }
}
