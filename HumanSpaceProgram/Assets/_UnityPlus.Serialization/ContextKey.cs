using System;
using System.Diagnostics;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A type-safe wrapper for Serialization Context IDs.
    /// </summary>
    [DebuggerDisplay( "{ToString()} (ID: {ID})" )]
    public readonly struct ContextKey : IEquatable<ContextKey>
    {
        public static readonly ContextKey Default = new ContextKey( 0 );

        public readonly int ID;

        /// <summary>
        /// Gets the C# type associated with this context.
        /// </summary>
        public Type Type => ContextRegistry.GetContextType( this );

        public bool IsGenericContext => ContextRegistry.IsGenericContext( this );

        public ContextKey( int id )
        {
            ID = id;
        }

        public bool TryGetGenericContextArguments( out ContextKey[] args )
            => ContextRegistry.TryGetGenericContextArguments( this, out args );

        public bool Equals( ContextKey other ) => ID == other.ID;
        public override bool Equals( object obj ) => obj is ContextKey other && Equals( other );
        public override int GetHashCode() => ID;
        public static bool operator ==( ContextKey left, ContextKey right ) => left.Equals( right );
        public static bool operator !=( ContextKey left, ContextKey right ) => !left.Equals( right );

        public override string ToString()
        {
            return ContextRegistry.GetContextName( this );
        }

        public static implicit operator ContextKey( int id ) => new ContextKey( id );
    }
}