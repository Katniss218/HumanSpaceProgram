using System;
using UnityPlus.Serialization;

namespace HSP.Content
{
    /// <summary>
    /// Represents a version constraint that can specify minimum and/or maximum version bounds.
    /// </summary>
    [Serializable]
    public readonly struct VersionConstraint
    {
        /// <summary>
        /// The minimum version (inclusive). Null means no minimum.
        /// </summary>
        public Version? MinVersion { get; }

        /// <summary>
        /// The maximum version (exclusive). Null means no maximum.
        /// </summary>
        public Version? MaxVersion { get; }

        public VersionConstraint( Version? minVersion, Version? maxVersion )
        {
            this.MinVersion = minVersion;
            this.MaxVersion = maxVersion;
        }

        public VersionConstraint( Version exactVersion )
        {
            this.MinVersion = exactVersion;
            this.MaxVersion = new Version( exactVersion.Major, exactVersion.Minor + 1 );
        }

        /// <summary>
        /// Checks if the given version satisfies this constraint.
        /// </summary>
        public bool IsSatisfiedBy( Version version )
        {
            if( MinVersion.HasValue && version < MinVersion.Value )
                return false;

            if( MaxVersion.HasValue && version >= MaxVersion.Value )
                return false;

            return true;
        }

        /// <summary>
        /// Creates a constraint that requires exactly the given version.
        /// </summary>
        public static VersionConstraint EqualTo( Version version )
        {
            return new VersionConstraint( version, new Version( version.Major, version.Minor + 1 ) );
        }

        public override string ToString()
        {
            if( MinVersion.HasValue && MaxVersion.HasValue )
            {
                return $">={MinVersion.Value},<{MaxVersion.Value}";
            }
            else if( MinVersion.HasValue )
            {
                return $">={MinVersion.Value}";
            }
            else if( MaxVersion.HasValue )
            {
                return $"<{MaxVersion.Value}";
            }
            else
            {
                return "any";
            }
        }

        /// <summary>
        /// Parses a version constraint string.
        /// </summary>
        /// <param name="constraintString">String in format ">=1.0,<2.0" or ">=1.0" or "<2.0" or "1.0" or "any", whitespace allowed between the versions.</param>
        public static VersionConstraint Parse( string constraintString )
        {
            if( string.IsNullOrWhiteSpace( constraintString ) || constraintString == "any" )
            {
                return new VersionConstraint( null, null );
            }
#warning TODO - make more robust.
            string[] parts = constraintString.Split( ',' );
            Version? minVersion = null;
            Version? maxVersion = null;

            foreach( string part in parts )
            {
                string trimmed = part.Trim();

                if( trimmed.StartsWith( ">=" ) )
                {
                    string versionStr = trimmed[2..];
                    minVersion = Version.Parse( versionStr );
                }
                else if( trimmed.StartsWith( "<" ) )
                {
                    string versionStr = trimmed[1..];
                    maxVersion = Version.Parse( versionStr );
                }
                else if( !trimmed.Contains( ">" ) && !trimmed.Contains( "<" ) )
                {
                    // Exact version
                    Version exactVersion = Version.Parse( trimmed );
                    return new VersionConstraint( exactVersion );
                }
            }

            return new VersionConstraint( minVersion, maxVersion );
        }

        public override bool Equals( object obj )
        {
            if( obj is VersionConstraint other )
            {
                return this.MinVersion == other.MinVersion && this.MaxVersion == other.MaxVersion;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( MinVersion, MaxVersion );
        }

        public static bool operator ==( VersionConstraint left, VersionConstraint right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( VersionConstraint left, VersionConstraint right )
        {
            return !left.Equals( right );
        }

        [MapsInheritingFrom( typeof( VersionConstraint ) )]
        public static SerializationMapping VersionConstraintMapping()
        {
            return new PrimitiveSerializationMapping<VersionConstraint>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString(),
                OnLoad = ( data, l ) => VersionConstraint.Parse( (string)data )
            };
        }
    }
}