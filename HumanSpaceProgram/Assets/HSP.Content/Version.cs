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
    public readonly struct Version
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
            return Major.ToString() + "." + Minor.ToString();
        }

        /// <summary>
        /// Parses a version from its string representation.
        /// </summary>
        /// <exception cref="ArgumentException">The string doesn't match any known valid version format.</exception>
        public static Version Parse( string s )
        {
            string[] parts = s.Split( '.' );
            if( parts.Length != 2 || !int.TryParse( parts[0], out int major ) || !int.TryParse( parts[1], out int minor ) )
            {
                throw new ArgumentException( "String to parse must be a valid version ('n.n').", nameof( s ) );
            }

            return new Version( major, minor );
        }

        /// <summary>
        /// Attempts to parse a version from its string representation.
        /// </summary>
        public static bool TryParse( string s, out Version result )
        {
            string[] parts = s.Split( '.' );
            if( parts.Length != 2 || !int.TryParse( parts[0], out int major ) || !int.TryParse( parts[1], out int minor ) )
            {
                result = default;
                return false;
            }

            result = new Version( major, minor );
            return true;
        }

        public static bool operator <( Version v1, Version v2 )
        {
            return v1.Major < v2.Major || (v1.Major == v2.Major && v1.Minor < v2.Minor);
        }

        public static bool operator >( Version v1, Version v2 )
        {
            return v1.Major > v2.Major || (v1.Major == v2.Major && v1.Minor > v2.Minor);
        }

        public static bool operator <=( Version v1, Version v2 )
        {
            return v1.Major < v2.Major || (v1.Major == v2.Major && v1.Minor <= v2.Minor);
        }

        public static bool operator >=( Version v1, Version v2 )
        {
            return v1.Major > v2.Major || (v1.Major == v2.Major && v1.Minor >= v2.Minor);
        }


        public static bool operator ==( Version v1, Version v2 )
        {
            return v1.Major == v2.Major && v1.Minor == v2.Minor;
        }

        public static bool operator !=( Version v1, Version v2 )
        {
            return v1.Major != v2.Major || v1.Minor != v2.Minor;
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