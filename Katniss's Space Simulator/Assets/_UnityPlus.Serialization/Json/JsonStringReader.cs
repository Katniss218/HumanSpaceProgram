using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Json
{
    public class JsonStringReader
    {
        // make a custom parser + reader/writer.

        string _s;
        int _pos;

        char? _currentChar;

        public JsonStringReader( string json )
        {
            this._s = json;
            this._pos = 0;

            UpdateCharacterCache();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void Advance( int num = 1 )
        {
            _pos += num;

            UpdateCharacterCache();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void UpdateCharacterCache()
        {
            if( _pos < 0 || _pos >= _s.Length )
                _currentChar = null;
            else
                _currentChar = _s[_pos];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private bool SeekCompare( string target )
        {
            if( _pos + target.Length > _s.Length )
                return false;

            return _s.Substring( _pos, target.Length ) == target;
            //return _s[(_pos)..(_pos + target.Length)] == target;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string Seek( int startOffset, int length )
        {
            if( _pos - startOffset < 0 || _pos + startOffset + length > _s.Length )
                return null;

            return _s.Substring( _pos + startOffset, length );
        }

        public SerializedData Read()
        {
            EatWhiteSpace();

            SerializedData val = EatValue();

            EatWhiteSpace();

            return val;
        }


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EatWhiteSpace()
        {
            while( _currentChar != null && char.IsWhiteSpace( _currentChar.Value ) )
            {
                Advance();
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void EatJsonText()
        {
            EatWhiteSpace();

            EatValue();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ArrayEnd()
        {
            EatWhiteSpace();

            if( _currentChar != ']' )
                throw new InvalidOperationException( $"Invalid token, expected `,` or `]`, but found `{_currentChar}`. {_pos}." );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_ObjectEnd()
        {
            EatWhiteSpace();

            if( _currentChar != '}' )
                throw new InvalidOperationException( "Invalid token, expected `,` or `}`, but found " + $"`{_currentChar}`. {_pos}." );
            Advance();

            EatWhiteSpace();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Eat_NameSeparator()
        {
            EatWhiteSpace();

            if( _currentChar != ':' )
                throw new InvalidOperationException( $"Invalid token, expected `:` after a name, but found `{_currentChar}`. {_pos}." );
            Advance();

            EatWhiteSpace();
        }

        public SerializedData EatValue()
        {
            if( SeekCompare( "false" ) )
            {
                Advance( "false".Length );
                return (SerializedPrimitive)false;
            }
            if( SeekCompare( "true" ) )
            {
                Advance( "true".Length );
                return (SerializedPrimitive)true;
            }
            if( SeekCompare( "null" ) )
            {
                Advance( "null".Length );
                return (SerializedPrimitive)null;
            }
            if( _currentChar == '[' )
            {
                return EatArray();
            }
            if( _currentChar == '{' )
            {
                return EatObject();
            }
            if( _currentChar == '"' )
            {
                return EatString();
            }
            if( _currentChar == '-' || (_currentChar != null && char.IsDigit( _currentChar.Value )) )
                return EatNumber();

            throw new InvalidOperationException( $"Unexpected token at {_pos}." );
        }

        public SerializedObject EatObject()
        {
            Contract.Assert( _currentChar == '{' );
            Advance();

            EatWhiteSpace();

            SerializedObject obj = new SerializedObject();

            if( _currentChar == '}' )
            {
                Eat_ObjectEnd();
                return obj;
            }

            while( true )
            {
                (string name, SerializedData val) = EatMember();
                obj.Add( name, val );

                // value sep
                EatWhiteSpace();

                if( _currentChar == ',' )
                {
                    Advance();

                    EatWhiteSpace();
                    continue;
                }

                // current char assumed to be `}`, since it was not `,`, so there is no next value
                break;
            }

            Eat_ObjectEnd();

            return obj;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public (string, SerializedData) EatMember()
        {
            string name = EatString();

            Eat_NameSeparator();

            SerializedData val = EatValue();

            return (name, val);
        }

        public SerializedArray EatArray()
        {
            Contract.Assert( _currentChar == '[' );
            Advance();

            EatWhiteSpace();

            SerializedArray arr = new SerializedArray();

            if( _currentChar == ']' )
            {
                Eat_ArrayEnd();
                return arr;
            }

            while( true )
            {
                SerializedData val = EatValue();
                arr.Add( val );

                // value sep
                EatWhiteSpace();

                if( _currentChar == ',' )
                {
                    Advance();

                    EatWhiteSpace();
                    continue;
                }

                // current char assumed to be `]`, since it was not `,`, so there is no next value
                break;
            }

            Eat_ArrayEnd();

            return arr;
        }

        public SerializedPrimitive EatString()
        {
            Contract.Assert( _currentChar == '"' );
            Advance();

            int start = _pos;
            StringBuilder sb = null;

            // Unescaped quote means the end of string
            while( _currentChar != '"' )
            {
                if( _currentChar == '\\' )
                {
                    if( sb == null )
                    {
                        sb = new StringBuilder();
                    }
                    int len = _pos - start;
                    if( len > 0 )
                    {
                        sb.Append( _s.Substring( start, len ) );
                    }

                    string seeked = Seek( 0, 2 );
                    if( seeked[1] == '\\' )
                        sb.Append( '\\' );
                    else if( seeked[1] == 'n' )
                        sb.Append( '\n' );
                    else if( seeked[1] == 'r' )
                        sb.Append( '\r' );
                    else if( seeked[1] == 'f' )
                        sb.Append( '\f' );
                    else if( seeked[1] == 'b' )
                        sb.Append( '\b' );
                    else if( seeked[1] == 't' )
                        sb.Append( '\t' );
                    else if( seeked[1] == 'u' )
                    {
                        string s = Seek( 2, 4 );
                        if( s == null )
                            throw new InvalidOperationException( $"Expected an escaped unicode char in the format `\\uNNNN`, where N is a digit 0-9. {_pos}" );

                        foreach( var ch in s )
                        {
                            if( !char.IsDigit( ch ) )
                                throw new InvalidOperationException( $"Expected an escaped unicode char in the format `\\uNNNN`, where N is a digit 0-9. {_pos}" );
                        }

                        // digit chars have a continuous underlying int value, and length is fixed,
                        // so we can hardcode that by casting the char to int, subtracting int 48, and multiplying that by position base 16
                        // I wonder if that's how int.Parse does it or not.
                        char c = (char)int.Parse( s, NumberStyles.HexNumber );
                        sb.Append( c );
                        Advance( 4 );
                    }
                    else
                        throw new InvalidOperationException( $"Expected an escaped unicode char. {_pos}" );
                    Advance( 2 );
                    start = _pos;
                }
                else
                {
                    Advance();
                }
            }

            int len2 = _pos - start - 1;
            //string val = _s[start.._pos];
            if( sb != null && len2 > 0 ) // append last section, if not empty
                sb.Append( _s.Substring( start + 1, len2 ) );

            string val = sb == null ? _s.Substring( start, _pos - start ) : sb.ToString();

            Contract.Assert( _currentChar == '"' );
            Advance();

            return (SerializedPrimitive)val;
        }

        public SerializedPrimitive EatNumber()
        {
            int start = _pos;
            bool hasDecimalPoint = false;
            bool hasExponent = false;

            if( _currentChar == '-' )
                Advance();

            EatInt();

            if( _currentChar == '.' )
            {
                hasDecimalPoint = true;
                Advance();

                if( _currentChar == null || !char.IsDigit( _currentChar.Value ) )
                {
                    throw new InvalidOperationException( $"Invalid token, a decimal point must be succeeded by a digit - {_pos}." );
                }

                EatInt();
            }

            if( _currentChar == 'e' || _currentChar == 'E' )
            {
                hasExponent = true;
                Advance();

                if( _currentChar != '+' && _currentChar != '-' )
                {
                    throw new InvalidOperationException( $"Invalid token, exponent 'e' must be succeeded by a plus/minus and a digit - {_pos}." );
                }

                Advance();

                if( _currentChar == null || !char.IsDigit( _currentChar.Value ) )
                {
                    throw new InvalidOperationException( $"Invalid token, exponent 'e' must be succeeded by a plus/minus and a digit - {_pos}." );
                }

                EatInt();

            }

            string val = _s[start..(_pos)];

            return (hasDecimalPoint || hasExponent)
                ? (SerializedPrimitive)double.Parse( val, CultureInfo.InvariantCulture )
                : (SerializedPrimitive)long.Parse( val, CultureInfo.InvariantCulture );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void EatInt()
        {
            while( _currentChar != null && char.IsDigit( _currentChar.Value ) )
            {
                Advance();
            }
        }
    }
}
