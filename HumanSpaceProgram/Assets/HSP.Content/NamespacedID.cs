using System;
using System.IO;
using UnityPlus.Serialization;

namespace HSP.Content
{
    /// <summary>
    /// An identifier that consists of a namespace (mod id) and a name (content id).
    /// </summary>
    [Serializable]
    public struct NamespacedID
    {
        const string SEPARATOR = "::";

        public string ModID { get; }
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

        public override int GetHashCode()
        {
            return HashCode.Combine( ModID.GetHashCode(), ContentID.GetHashCode() );
        }

        public override bool Equals( object obj )
        {
            if( obj is NamespacedID other )
                return this.ModID == other.ModID
                    && this.ContentID == other.ContentID;

            return false;
        }

        public override string ToString()
        {
            return ModID + SEPARATOR + ContentID;
        }

        public static NamespacedID Parse( string str )
        {
            string[] parts = str.Split( SEPARATOR );

            if( parts.Length != 2 )
            {
                throw new ArgumentException( $"Namespaced string must contain modId and contentId, separated by `{SEPARATOR}`.", nameof( str ) );
            }
            if( string.IsNullOrEmpty( parts[0] ) || string.IsNullOrEmpty( parts[0] ) )
            {
                throw new ArgumentException( $"modId and contentId must both be a nonnull, nonzero length strings.", nameof( str ) );
            }

            return new NamespacedID( parts[0], parts[1] );
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
        public string ToContentPath( string contentType )
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

        public static explicit operator NamespacedID( string str )
        {
            return NamespacedID.Parse( str );
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