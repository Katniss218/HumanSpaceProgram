using System;
using System.IO;
using UnityPlus.Serialization;

namespace HSP.Content
{
    /// <summary>
    /// An identifier that consists of a namespace (mod id) and a name (content id).
    /// </summary>
    [Serializable]
    public struct NamespacedID : IEquatable<NamespacedID>
    {
        /// <summary>
        /// Separates the ModID and ContentID in a string representation.
        /// </summary>
        public const string SEPARATOR = "::";

        /// <summary>
        /// The first part of the ID, representing the mod that provides the content.
        /// </summary>
        public string ModID { get; }

        /// <summary>
        /// The second part of the ID, representing the specific content provided by the mod.
        /// </summary>
        public string ContentID { get; }

        public NamespacedID( string modId, string contentId )
        {
            if( string.IsNullOrEmpty( modId ) )
            {
                throw new ArgumentException( $"{nameof( modId )} and {nameof( contentId )} must both be nonnull, nonzero length strings.", nameof( modId ) );
            }
            if( string.IsNullOrEmpty( contentId ) )
            {
                throw new ArgumentException( $"{nameof( modId )} and {nameof( contentId )} must both be nonnull, nonzero length strings.", nameof( contentId ) );
            }

            this.ModID = modId;
            this.ContentID = contentId;
        }

        public readonly bool Equals( NamespacedID other )
        {
            return this == other;
        }

        public override readonly bool Equals( object obj )
        {
            if( obj is not NamespacedID id )
            {
                return false;
            }
            return this == id;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine( ModID.GetHashCode(), ContentID.GetHashCode() );
        }

        public override readonly string ToString()
        {
            return ModID + SEPARATOR + ContentID;
        }

        public static NamespacedID Parse( string s )
        {
            string[] parts = s.Split( SEPARATOR );

            if( parts.Length != 2 )
            {
                throw new ArgumentException( $"Namespaced string must contain modId and contentId, separated by `{SEPARATOR}`.", nameof( s ) );
            }
            if( string.IsNullOrEmpty( parts[0] ) || string.IsNullOrEmpty( parts[1] ) )
            {
                throw new ArgumentException( $"modId and contentId must both be a nonnull, nonzero length strings.", nameof( s ) );
            }

            return new NamespacedID( parts[0], parts[1] );
        }
        
        public static bool TryParse( string s, out NamespacedID result )
        {
            string[] parts = s.Split( SEPARATOR );

            if( parts.Length != 2 || string.IsNullOrEmpty( parts[0] ) || string.IsNullOrEmpty( parts[1] ) )
            {
                result = default;
                return false;
            }

            result = new NamespacedID( parts[0], parts[1] );
            return true;
        }

        /// <summary>
        /// Gets the namespaced ID corresponding to the specified content inside HSP's content directory.
        /// </summary>
        /// <param name="contentPath">The path to the content, in the form of `GameData/{mod_id}/{content_type}/{content_id}`.</param>
        /// <param name="contentType">The type of the content (see <paramref name="contentPath"/>)</param>
        /// <returns>The namespaced ID corresponding to the specified content.</returns>
        public static NamespacedID FromContentPath( string contentPath, out string contentType )
        {
            if( string.IsNullOrWhiteSpace( contentPath ) )
                throw new ArgumentException( "Content path cannot be null or empty", nameof( contentPath ) );

            contentPath = contentPath.Replace( '\\', '/' );
            string contentDirectory = HumanSpaceProgramContent.GetContentDirectoryPath();

            string localContentPath = Path.GetRelativePath( contentDirectory, contentPath ).Replace( '\\', '/' ); ;
            if( string.IsNullOrEmpty( localContentPath ) )
            {
                throw new ArgumentException( $"Invalid content path '{contentPath}'.", nameof( contentPath ) );
            }

            string[] parts = localContentPath.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );

            if( parts.Length < 3 ) // ../mod_id/ContentType/content_id
            {
                throw new ArgumentException( $"Invalid content path '{contentPath}'.", nameof( contentPath ) );
            }

            string modId = parts[0];
            contentType = parts[1];
            string contentId = parts[2];

            return new NamespacedID( modId, contentId );
        }

        /// <summary>
        /// Gets the path to content inside HSP's content directory corresponding to the specified namespaced ID.
        /// </summary>
        /// <param name="contentType">The type of the content, see: `GameData/{mod_id}/{content_type}/{content_id}`</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public readonly string ToContentPath( string contentType )
        {
            if( string.IsNullOrWhiteSpace( contentType ) )
                throw new ArgumentException( "Content type cannot be null or empty", nameof( contentType ) );

            string path = Path.Combine( HumanSpaceProgramContent.GetContentDirectoryPath(), this.ModID, contentType, this.ContentID );

            if( !path.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
            {
                path += Path.DirectorySeparatorChar;
            }

            return path;
        }

        public static explicit operator NamespacedID( string s )
        {
            return NamespacedID.Parse( s );
        }

        public static bool operator ==( NamespacedID left, NamespacedID right )
        {
            return left.ModID == right.ModID
                && left.ContentID == right.ContentID;
        }
        public static bool operator !=( NamespacedID left, NamespacedID right )
        {
            return left.ModID != right.ModID
                || left.ContentID != right.ContentID;
        }

        [MapsInheritingFrom( typeof( NamespacedID ) )]
        public static SerializationMapping NamespacedIdentifierMapping()
        {
            return new PrimitiveSerializationMapping<NamespacedID>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString(),
                OnLoad = ( data, l ) => NamespacedID.Parse( (string)data )
            };
        }
    }
}