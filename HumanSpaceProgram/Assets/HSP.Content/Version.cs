using System;
using UnityPlus.Serialization;

namespace HSP.Content
{
    /// <summary>
    /// Represents a version where the major number indicates breaking changes, and minor indicates non-breaking changes.
    /// </summary>
    /// <remarks>
    /// Can be used to compare if 2 versions are compatible with each other, or if breaking changes occurred.
    /// </remarks>
    public struct Version
    {
        /// <summary>
        /// The number indicating the major (breaking) part of the version.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The number indicating the minor (non-breaking) part of the version.
        /// </summary>
        public int Minor { get; }

        public Version( int major, int minor )
        {
            this.Major = major;
            this.Minor = minor;
        }

        /// <summary>
        /// Checks if two versions (of the same thing) are compatible with each other.
        /// </summary>
        /// <remarks>
        /// Versions are compatible if they don't have any breaking changes between them (major is the same).
        /// </remarks>
        /// <param name="v1">The first version.</param>
        /// <param name="v2">The second version.</param>
        /// <returns>True if the versions are compatible. False otherwise.</returns>
        public static bool AreCompatible( Version v1, Version v2 )
        {
            return v1.Major == v2.Major;
        }

        public static bool operator <( Version v1, Version v2 )
        {
            if( v1.Major < v2.Major ) return true;
            if( v1.Major > v2.Major ) return false;
            return v1.Minor < v2.Minor;
        }

        public static bool operator >( Version v1, Version v2 )
        {
            if( v1.Major > v2.Major ) return true;
            if( v1.Major < v2.Major ) return false;
            return v1.Minor > v2.Minor;
        }

        public static bool operator ==( Version v1, Version v2 )
        {
            return v1.Minor == v2.Minor && v1.Major == v2.Major;
        }

        public static bool operator !=( Version v1, Version v2 )
        {
            return v1.Minor != v2.Minor || v1.Major != v2.Major;
        }

        public override bool Equals( object obj )
        {
            if( obj is not Version v )
            {
                return false;
            }
            return this == v;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( Major, Minor );
        }

        public override string ToString()
        {
            return Major.ToString( "#########0" ) + "." + Minor.ToString( "#########0" );
        }

        /// <summary>
        /// Parses a version from its string representation.
        /// </summary>
        /// <exception cref="ArgumentException">The string doesn't match any known valid version format.</exception>
        public static Version Parse( string s )
        {
            string[] strings = s.Split( '.' );
            if( strings.Length != 2 )
            {
                throw new ArgumentException( $"String to parse must be a valid version ('n.n').", nameof( s ) );
            }
            int major = int.Parse( strings[0] );
            int minor = int.Parse( strings[1] );
            return new Version( major, minor );
        }


        [MapsInheritingFrom( typeof( Version ) )]
        public static SerializationMapping VersionMapping()
        {
            return new PrimitiveSerializationMapping<Version>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString(),
                OnLoad = ( data, l ) => Version.Parse( (string)data )
            };
        }
    }
}