
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents a compiled path to access one or multiple child elements of a serialized data object.
    /// Supports dot notation, indexers, ranges, and wildcards.
    /// </summary>
    /// <example>
    /// Path Examples:
    /// - "stats.health"
    /// - "inventory.items[0]"
    /// - "data[*]"
    /// - "history[0..5]"
    /// </example>
    public readonly struct SerializedDataPath : IEquatable<SerializedDataPath>
    {
        private readonly SerializedDataPathSegment[] _segments;

        public IReadOnlyList<SerializedDataPathSegment> Segments => _segments;

        public SerializedDataPath( params SerializedDataPathSegment[] segments )
        {
            _segments = segments ?? Array.Empty<SerializedDataPathSegment>();
        }

        public SerializedDataPath( IEnumerable<SerializedDataPathSegment> segments )
        {
            _segments = (segments ?? Enumerable.Empty<SerializedDataPathSegment>()).ToArray();
        }

        /// <summary>
        /// Evaluates a path on a single root node.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( SerializedData root )
        {
            return Evaluate( new TrackedSerializedData( root ) );
        }

        /// <summary>
        /// Evaluates a path on a single pivot.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            if( _segments == null || _segments.Length == 0 )
                return new TrackedSerializedData[] { pivotItem };

            IEnumerable<TrackedSerializedData> pivot = _segments[0].Evaluate( pivotItem );
            for( int i = 1; i < _segments.Length; i++ )
            {
                pivot = _segments[i].Evaluate( pivot );
            }
            return pivot;
        }

        /// <summary>
        /// Evaluates a path on multiple pivots, returns a flattened sequence of results.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            if( _segments == null || _segments.Length == 0 )
                return pivot;

            foreach( var segment in _segments )
            {
                pivot = segment.Evaluate( pivot );
            }
            return pivot;
        }

        public bool Equals( SerializedDataPath other )
        {
            if( ReferenceEquals( this, other ) ) return true;
            if( _segments.Length != other._segments.Length ) return false;

            for( int i = 0; i < _segments.Length; i++ )
            {
                if( !_segments[i].Equals( other._segments[i] ) )
                    return false;
            }
            return true;
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedDataPath other ) return Equals( other );
            return false;
        }

        public override int GetHashCode()
        {
            // Simple hash combination
            int hash = 17;
            for( int i = 0; i < _segments.Length; i++ )
                hash = hash * 31 + _segments[i].GetHashCode();
            return hash;
        }

        public static bool TryParse( string s, out SerializedDataPath path )
        {
            try
            {
                path = Parse( s );
                return true;
            }
            catch( Exception )
            {
                path = default;
                return false;
            }
        }

        public static SerializedDataPath Parse( string s )
        {
            if( s == null ) throw new ArgumentNullException( nameof( s ) );

            int pos = 0;

            bool EatPrefix( out SerializedDataPathSegment segment )
            {
                segment = default;
                if( s.Length - pos >= 3 )
                {
                    if( s[pos] == 'a' && s[pos + 1] == 'n' && s[pos + 2] == 'y' &&
                        (s.Length - pos == 3 || !IsIdentifierChar( s[pos + 3] )) )
                    {
                        segment = SerializedDataPathSegment.Any();
                        pos += 3;
                        return true;
                    }
                }

                if( s.Length - pos >= 4 )
                {
                    if( s[pos] == 't' && s[pos + 1] == 'h' && s[pos + 2] == 'i' && s[pos + 3] == 's' &&
                        (s.Length - pos == 4 || !IsIdentifierChar( s[pos + 4] )) )
                    {
                        segment = SerializedDataPathSegment.This();
                        pos += 4;
                        return true;
                    }
                }
                return false;
            }

            bool EatDot()
            {
                if( pos + 1 < s.Length && s[pos] == '.' )
                {
                    pos++;
                    return true;
                }
                return false;
            }

            bool EatQuotedString( out SerializedDataPathSegment segment )
            {
                segment = default;
                if( pos >= s.Length || s[pos] != '"' ) return false;
                pos++;

                var sb = new StringBuilder();
                while( pos < s.Length )
                {
                    char c = s[pos++];
                    if( c == '"' )
                    {
                        segment = SerializedDataPathSegment.Named( sb.ToString() );
                        return true;
                    }
                    if( c == '\\' )
                    {
                        if( pos >= s.Length ) throw new FormatException( "Invalid escape sequence at end of input." );
                        char esc = s[pos++];
                        switch( esc )
                        {
                            case '"': sb.Append( '"' ); break;
                            case '\\': sb.Append( '\\' ); break;
                            default: sb.Append( esc ); break; // Simple handling
                        }
                    }
                    else
                    {
                        sb.Append( c );
                    }
                }
                throw new FormatException( "Unterminated quoted string." );
            }

            bool EatBracketedIndexer( out SerializedDataPathSegment segment )
            {
                segment = default;
                if( pos >= s.Length || s[pos] != '[' ) return false;
                pos++;

                int startInner = pos;
                int bracketDepth = 1;
                while( pos < s.Length && bracketDepth > 0 )
                {
                    if( s[pos] == ']' ) bracketDepth--;
                    else if( s[pos] == '[' ) bracketDepth++;
                    pos++;
                }

                if( pos >= s.Length || s[pos - 1] != ']' ) throw new FormatException( "Unterminated bracket." );

                int endInner = pos - 1;
                string inner = s[startInner..endInner].Trim();

                if( inner.Length == 0 ) throw new FormatException( "Empty indexer." );

                if( inner == "*" )
                {
                    segment = SerializedDataPathSegment.IndexedAll();
                    return true;
                }

                int rangeSeparator = inner.IndexOf( ".." );
                if( rangeSeparator >= 0 )
                {
                    string rangeOnly = inner;
                    int step = 1;
                    int colonIdx = inner.IndexOf( ':' );
                    if( colonIdx >= 0 )
                    {
                        rangeOnly = inner[..colonIdx].Trim();
                        string stepPart = inner[(colonIdx + 1)..].Trim();
                        if( !int.TryParse( stepPart, out step ) || step <= 0 )
                            throw new FormatException( $"Invalid step '{stepPart}'." );
                    }

                    string left = rangeOnly[..rangeSeparator].Trim();
                    string right = rangeOnly[(rangeSeparator + 2)..].Trim();

                    int indexMin = (left.Length > 0) ? int.Parse( left ) : 0;
                    int indexMax = (right.Length > 0) ? int.Parse( right ) : int.MaxValue;

                    segment = SerializedDataPathSegment.IndexedRange( indexMin, indexMax, step );
                    return true;
                }

                if( !int.TryParse( inner, out int idx ) )
                    throw new FormatException( $"Invalid index '{inner}'." );

                segment = SerializedDataPathSegment.Indexed( idx );
                return true;
            }

            bool EatNamedChild( out SerializedDataPathSegment segment )
            {
                segment = default;
                if( pos >= s.Length ) return false;
                int start = pos;
                if( !IsIdentifierChar( s[pos] ) ) return false;
                pos++;
                while( pos < s.Length && IsIdentifierChar( s[pos] ) ) pos++;
                string name = s[start..pos];
                segment = SerializedDataPathSegment.Named( name );
                return true;
            }

            static bool IsIdentifierChar( char c )
            {
                return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_';
            }

            List<SerializedDataPathSegment> segments = new();

            if( EatPrefix( out var prefix ) ) segments.Add( prefix );

            while( pos < s.Length )
            {
                if( s[pos] == '[' )
                {
                    if( !EatBracketedIndexer( out var idxSeg ) ) throw new FormatException( "Failed to parse indexer." );
                    segments.Add( idxSeg );
                    continue;
                }

                if( segments.Count > 0 && !EatDot() )
                    throw new FormatException( $"Expected '.' or '[' at position {pos}." );

                if( s[pos] == '"' )
                {
                    if( !EatQuotedString( out var quoted ) ) throw new FormatException( "Failed to parse quoted string." );
                    segments.Add( quoted );
                    continue;
                }

                if( EatNamedChild( out var named ) )
                {
                    segments.Add( named );
                    continue;
                }

                throw new FormatException( $"Unexpected character '{s[pos]}' at position {pos}." );
            }

            return new SerializedDataPath( segments );
        }

        public override string ToString()
        {
            if( _segments == null || _segments.Length == 0 ) return string.Empty;
            var sb = new StringBuilder();
            for( int i = 0; i < _segments.Length; i++ )
            {
                if( i > 0 && _segments[i].Kind == SerializedDataPathSegment.KindEnum.Named ) sb.Append( '.' );
                sb.Append( _segments[i].ToString() );
            }
            return sb.ToString();
        }
    }
}