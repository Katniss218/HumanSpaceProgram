using System;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Immutable, normalized identifier for an encoding format.
    /// </summary>
    public readonly struct AssetFormat : IEquatable<AssetFormat>
    {
        private readonly string _id;

        public string Id => _id;

        public static readonly AssetFormat Unknown = new AssetFormat( string.Empty );

        public AssetFormat( string id )
        {
            if( string.IsNullOrWhiteSpace( id ) )
            {
                _id = string.Empty;
                return;
            }

            string s = id.Trim();

            // Normalize: leading dot, uppercase
            if( !s.StartsWith( ".", StringComparison.Ordinal ) )
                s = "." + s;

            _id = s.ToUpperInvariant();
        }

        public static AssetFormat FromExtension( string ext )
        {
            if( string.IsNullOrWhiteSpace( ext ) )
                return Unknown;

            return AssetFormatRegistry.FromExtension( ext );
        }

        public static AssetFormat FromMimeType( string mimeType )
        {
            return AssetFormatRegistry.FromMimeType( mimeType );
        }

        public static AssetFormat FromStream( System.IO.Stream stream )
        {
            if( stream == null )
                return Unknown;

            return AssetFormatRegistry.FromStream( stream );
        }

        public static AssetFormat FromByteArray( byte[] data )
        {
            return AssetFormatRegistry.FromByteArray( data );
        }

        public bool Equals( AssetFormat other )
            => string.Equals( _id, other._id, StringComparison.OrdinalIgnoreCase );

        public override bool Equals( object obj )
            => obj is AssetFormat other && Equals( other );

        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode( _id ?? string.Empty );

        public override string ToString() => _id;

        public static bool operator ==( AssetFormat left, AssetFormat right ) => left.Equals( right );
        public static bool operator !=( AssetFormat left, AssetFormat right ) => !left.Equals( right );

        public static explicit operator AssetFormat( string id ) => new AssetFormat( id );
        public static explicit operator string( AssetFormat fmt ) => fmt._id;
    }
}