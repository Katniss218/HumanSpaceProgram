
using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A single struct representation of all possible serialized-data path segments.
    /// Used to traverse or query the SerializedData DOM.
    /// </summary>
    public readonly struct SerializedDataPathSegment : IEquatable<SerializedDataPathSegment>
    {
        public enum KindEnum : byte
        {
            This,
            Any,
            Global,
            Named,
            Indexed
        }

        public KindEnum Kind { get; }

        // Named segment
        public string Name { get; }

        // Indexed segment fields
        public bool Every { get; }
        public int IndexMin { get; }
        public int IndexMax { get; }
        public int Step { get; }

        private SerializedDataPathSegment( KindEnum kind, string name = null, bool every = false, int indexMin = 0, int indexMax = 0, int step = 1 )
        {
            Kind = kind;
            Name = name;
            Every = every;
            IndexMin = indexMin;
            IndexMax = indexMax;
            Step = step;
        }

        public static SerializedDataPathSegment This() => new SerializedDataPathSegment( KindEnum.This );

        public static SerializedDataPathSegment Any() => new SerializedDataPathSegment( KindEnum.Any );

        public static SerializedDataPathSegment Global() => new SerializedDataPathSegment( KindEnum.Global );

        public static SerializedDataPathSegment Named( string name )
        {
            if( name == null )
                throw new ArgumentNullException( nameof( name ) );
            return new SerializedDataPathSegment( KindEnum.Named, name: name );
        }

        /// <summary>
        /// Single index.
        /// </summary>
        public static SerializedDataPathSegment Indexed( int index )
            => new SerializedDataPathSegment( KindEnum.Indexed, every: false, indexMin: index, indexMax: index, step: 1 );

        /// <summary>
        /// Range [indexMin, indexMax) with step (indexMax is treated as exclusive).
        /// </summary>
        public static SerializedDataPathSegment IndexedRange( int indexMin, int indexMax, int step = 1 )
            => new SerializedDataPathSegment( KindEnum.Indexed, every: false, indexMin: indexMin, indexMax: indexMax, step: step );

        /// <summary>
        /// All elements (the [*] operator).
        /// </summary>
        public static SerializedDataPathSegment IndexedAll()
            => new SerializedDataPathSegment( KindEnum.Indexed, every: true, indexMin: 0, indexMax: 0, step: 1 );

        /// <summary>
        /// Evaluate this segment for a single pivot item.
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( TrackedSerializedData pivotItem )
        {
            switch( Kind )
            {
                case KindEnum.This:
                    yield return pivotItem;
                    yield break;

                case KindEnum.Any:
                    foreach( var d in TraverseOne( pivotItem ) )
                        yield return d;
                    yield break;

                case KindEnum.Global:
                    yield return new TrackedSerializedData( pivotItem.Root );
                    yield break;

                case KindEnum.Named:
                    if( pivotItem.TryGetValue( Name, out var namedValue ) )
                        yield return namedValue;
                    yield break;

                case KindEnum.Indexed:
                    if( Every )
                    {
                        if( pivotItem.Value is SerializedArray arr )
                        {
                            for( int i = 0; i < arr.Count; i++ )
                            {
                                yield return new TrackedSerializedData( arr[i], pivotItem.Value, i, pivotItem.Root );
                            }
                        }
                        yield break;
                    }

                    if( IndexMin == IndexMax )
                    {
                        if( pivotItem.TryGetValue( IndexMin, out var single ) )
                            yield return single;
                        yield break;
                    }
                    else
                    {
                        if( pivotItem.Value is SerializedArray arr )
                        {
                            for( int i = IndexMin; i < IndexMax && i < arr.Count; i += Math.Max( 1, Step ) )
                            {
                                yield return new TrackedSerializedData( arr[i], pivotItem.Value, i, pivotItem.Root );
                            }
                        }
                        yield break;
                    }

                default:
                    yield break;
            }
        }

        /// <summary>
        /// Evaluate this segment for multiple pivots (flattened).
        /// </summary>
        public IEnumerable<TrackedSerializedData> Evaluate( IEnumerable<TrackedSerializedData> pivot )
        {
            if( pivot == null ) yield break;

            foreach( var p in pivot )
            {
                foreach( var result in Evaluate( p ) )
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<TrackedSerializedData> TraverseOne( TrackedSerializedData root )
        {
            // Depth-first traversal that returns descendants including immediate children.
            Stack<TrackedSerializedData> stack = new Stack<TrackedSerializedData>();
            stack.Push( root );

            while( stack.Count > 0 )
            {
                TrackedSerializedData current = stack.Pop();

                foreach( var child in current.EnumerateChildren() )
                {
                    yield return child;
                    stack.Push( child ); // Continue traversing this branch
                }
            }
        }

        public bool Equals( SerializedDataPathSegment other )
        {
            if( Kind != other.Kind )
                return false;

            switch( Kind )
            {
                case KindEnum.Named:
                    return string.Equals( Name, other.Name, StringComparison.Ordinal );
                case KindEnum.Indexed:
                    return Every == other.Every
                        && IndexMin == other.IndexMin
                        && IndexMax == other.IndexMax
                        && Step == other.Step;
                default:
                    return true;
            }
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedDataPathSegment other )
            {
                return Equals( other );
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Kind;
                if( Kind == KindEnum.Named && Name != null )
                    hash = (hash * 397) ^ Name.GetHashCode();
                if( Kind == KindEnum.Indexed )
                {
                    hash = (hash * 397) ^ Every.GetHashCode();
                    hash = (hash * 397) ^ IndexMin;
                    hash = (hash * 397) ^ IndexMax;
                    hash = (hash * 397) ^ Step;
                }
                return hash;
            }
        }

        public override string ToString()
        {
            switch( Kind )
            {
                case KindEnum.This: return "this";
                case KindEnum.Any: return "any";
                case KindEnum.Global: return "global";
                case KindEnum.Named:
                {
                    var name = Name ?? string.Empty;
                    if( !NameNeedsQuoting( name ) )
                        return name;
                    return QuoteAndEscape( name );
                }
                case KindEnum.Indexed:
                    if( Every ) return "[*]";
                    if( IndexMin == IndexMax ) return $"[{IndexMin}]";
                    return $"[{IndexMin}..{IndexMax}{(Step != 1 ? $":{Step}" : "")}]";
                default: return string.Empty;
            }
        }

        static bool NameNeedsQuoting( string name )
        {
            if( string.IsNullOrEmpty( name ) ) return true;
            foreach( char c in name )
            {
                if( char.IsWhiteSpace( c ) ) return true;
                switch( c )
                {
                    case '.':
                    case '[':
                    case ']':
                    case '"':
                    case '\\':
                        return true;
                }
            }
            return false;
        }

        static string QuoteAndEscape( string name )
        {
            var sb = new System.Text.StringBuilder();
            sb.Append( '"' );
            foreach( char c in name )
            {
                switch( c )
                {
                    case '\\': sb.Append( @"\\" ); break;
                    case '"': sb.Append( "\\\"" ); break;
                    case '\b': sb.Append( @"\b" ); break;
                    case '\f': sb.Append( @"\f" ); break;
                    case '\n': sb.Append( @"\n" ); break;
                    case '\r': sb.Append( @"\r" ); break;
                    case '\t': sb.Append( @"\t" ); break;
                    default:
                        if( char.IsControl( c ) || c < 32 )
                        {
                            sb.Append( "\\u" );
                            sb.Append( ((int)c).ToString( "X4" ) ); // 4-digit hex
                        }
                        else
                        {
                            sb.Append( c );
                        }
                        break;
                }
            }
            sb.Append( '"' );
            return sb.ToString();
        }

        public static bool operator ==( SerializedDataPathSegment a, SerializedDataPathSegment b ) => a.Equals( b );
        public static bool operator !=( SerializedDataPathSegment a, SerializedDataPathSegment b ) => !a.Equals( b );
    }
}