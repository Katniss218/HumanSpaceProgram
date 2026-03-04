using System;
using System.IO;
using System.Text;

namespace UnityPlus.Serialization.Json
{
    public static class JsonCommon
    {
        private static readonly char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public static void WriteEscapedString( string str, StringBuilder sb )
        {
            if( str == null )
            {
                sb.Append( "null" );
                return;
            }

            sb.Append( '"' );

            int len = str.Length;
            int lastIndex = 0;

            for( int i = 0; i < len; i++ )
            {
                char c = str[i];

                // Common case: No escaping needed
                if( c >= ' ' && c != '"' && c != '\\' )
                    continue;

                // Flush previous chunk
                if( i > lastIndex )
                {
                    sb.Append( str, lastIndex, i - lastIndex );
                }
                lastIndex = i + 1;

                switch( c )
                {
                    case '"': sb.Append( "\\\"" ); break;
                    case '\\': sb.Append( "\\\\" ); break;
                    case '\b': sb.Append( "\\b" ); break;
                    case '\f': sb.Append( "\\f" ); break;
                    case '\n': sb.Append( "\\n" ); break;
                    case '\r': sb.Append( "\\r" ); break;
                    case '\t': sb.Append( "\\t" ); break;
                    default:
                        sb.Append( "\\u" );
                        sb.Append( HexDigits[(c >> 12) & 0xF] );
                        sb.Append( HexDigits[(c >> 8) & 0xF] );
                        sb.Append( HexDigits[(c >> 4) & 0xF] );
                        sb.Append( HexDigits[c & 0xF] );
                        break;
                }
            }

            // Flush remaining
            if( len > lastIndex )
            {
                sb.Append( str, lastIndex, len - lastIndex );
            }

            sb.Append( '"' );
        }

        public static void WriteEscapedString( string str, TextWriter writer )
        {
            if( str == null )
            {
                writer.Write( "null" );
                return;
            }

            writer.Write( '"' );

            int len = str.Length;
            int lastIndex = 0;

            for( int i = 0; i < len; i++ )
            {
                char c = str[i];

                // Check if character needs escaping
                if( c >= ' ' && c != '"' && c != '\\' )
                    continue;

                // Write the clean chunk before this character
                if( i > lastIndex )
                {
                    // TextWriter.Write(string) is much faster than char-by-char
                    writer.Write( str.Substring( lastIndex, i - lastIndex ) );
                }
                lastIndex = i + 1;

                switch( c )
                {
                    case '"': writer.Write( "\\\"" ); break;
                    case '\\': writer.Write( "\\\\" ); break;
                    case '\b': writer.Write( "\\b" ); break;
                    case '\f': writer.Write( "\\f" ); break;
                    case '\n': writer.Write( "\\n" ); break;
                    case '\r': writer.Write( "\\r" ); break;
                    case '\t': writer.Write( "\\t" ); break;
                    default:
                        writer.Write( "\\u" );
                        writer.Write( HexDigits[(c >> 12) & 0xF] );
                        writer.Write( HexDigits[(c >> 8) & 0xF] );
                        writer.Write( HexDigits[(c >> 4) & 0xF] );
                        writer.Write( HexDigits[c & 0xF] );
                        break;
                }
            }

            if( len > lastIndex )
            {
                writer.Write( str.Substring( lastIndex, len - lastIndex ) );
            }

            writer.Write( '"' );
        }

        public static string UnescapeString( ReadOnlySpan<char> escaped )
        {
            bool needsUnescape = false;
            for( int i = 0; i < escaped.Length; i++ )
            {
                if( escaped[i] == '\\' )
                {
                    needsUnescape = true;
                    break;
                }
            }

            if( !needsUnescape )
                return escaped.ToString();

            StringBuilder sb = new StringBuilder( escaped.Length );
            for( int i = 0; i < escaped.Length; i++ )
            {
                char c = escaped[i];
                if( c == '\\' )
                {
                    i++;
                    if( i >= escaped.Length ) throw new FormatException( "Invalid escape sequence at end of string." );

                    switch( escaped[i] )
                    {
                        case '"': sb.Append( '"' ); break;
                        case '\\': sb.Append( '\\' ); break;
                        case '/': sb.Append( '/' ); break;
                        case 'b': sb.Append( '\b' ); break;
                        case 'f': sb.Append( '\f' ); break;
                        case 'n': sb.Append( '\n' ); break;
                        case 'r': sb.Append( '\r' ); break;
                        case 't': sb.Append( '\t' ); break;
                        case 'u':
                            if( i + 4 >= escaped.Length ) throw new FormatException( "Invalid unicode escape sequence." );

                            // Parse 4 hex digits manually to avoid allocation overhead of substring
                            int val = 0;
                            for( int j = 0; j < 4; j++ )
                            {
                                char h = escaped[i + 1 + j];
                                val <<= 4;
                                if( h >= '0' && h <= '9' ) val += h - '0';
                                else if( h >= 'a' && h <= 'f' ) val += h - 'a' + 10;
                                else if( h >= 'A' && h <= 'F' ) val += h - 'A' + 10;
                                else throw new FormatException( "Invalid hex digit in unicode escape." );
                            }
                            sb.Append( (char)val );
                            i += 4;
                            break;
                        default:
                            throw new FormatException( $"Invalid escape character: \\{escaped[i]}" );
                    }
                }
                else
                {
                    sb.Append( c );
                }
            }
            return sb.ToString();
        }
    }
}
