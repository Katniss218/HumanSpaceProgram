using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Content
{
    /// <summary>
    /// Represents a version constraint that can specify minimum and/or maximum version bounds.
    /// </summary>
    [Serializable]
    public readonly struct VersionConstraint : IEquatable<VersionConstraint>
    {
        /// <summary>
        /// The minimum version (inclusive). Null means no minimum.
        /// </summary>
        public Version? MinVersion { get; }

        /// <summary>
        /// The maximum version (exclusive). Null means no maximum.
        /// </summary>
        public Version? MaxVersion { get; }

        public static readonly VersionConstraint Any = new VersionConstraint( null, null );

        public VersionConstraint( Version? minVersion, Version? maxVersion )
        {
            if( minVersion.HasValue && maxVersion.HasValue && minVersion.Value >= maxVersion.Value )
            {
                throw new ArgumentException( "Minimum version must be strictly less than maximum version." );
            }

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

        public bool Equals( VersionConstraint other )
        {
            return MinVersion == other.MinVersion && MaxVersion == other.MaxVersion;
        }

        public override bool Equals( object obj )
        {
            return obj is VersionConstraint other && Equals( other );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( MinVersion, MaxVersion );
        }

        public override string ToString()
        {
            if( MinVersion.HasValue && MaxVersion.HasValue )
            {
                if( MinVersion.Value.Major == MaxVersion.Value.Major && MinVersion.Value.Minor + 1 == MaxVersion.Value.Minor )
                {
                    return MinVersion.Value.ToString();
                }

                return $">={MinVersion.Value}, <{MaxVersion.Value}";
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

        public static VersionConstraint Parse( string constraintString )
        {
            if( TryParse( constraintString, out VersionConstraint result ) )
            {
                return result;
            }

            throw new FormatException( $"Invalid version constraint format: '{constraintString}'" );
        }

        /// <summary>
        /// Parses a version constraint string.
        /// </summary>
        /// <param name="constraintString">String in format ">=1.0, <2.0" or ">=1.0" or "<2.0" or "1.0" or "any", whitespace allowed between the versions.</param>
        public static bool TryParse( string constraintString, out VersionConstraint result )
        {
            result = default;
            if( string.IsNullOrWhiteSpace( constraintString ) )
            {
                return false;
            }

            if( constraintString == "any" )
            {
                result = VersionConstraint.Any;
                return true;
            }

            string[] parts = constraintString.Split( ',' );
            Version? minVersion = null;
            Version? maxVersion = null;
            if( parts.Length > 2 )
            {
                return false;
            }

            // Exact version must be a single part.
            if( parts.Length == 1 && !parts[0].Contains( ">" ) && !parts[0].Contains( "<" ) )
            {
                if( !Version.TryParse( parts[0], out Version exactVersion ) )
                    return false;
                result = new VersionConstraint( exactVersion );
                return true;
            }

            foreach( string part in parts )
            {
                string trimmed = part.Trim();

                if( trimmed.StartsWith( ">=" ) )
                {
                    if( minVersion != null )
                        return false;

                    string versionStr = trimmed[2..];
                    if( !Version.TryParse( versionStr, out Version parsedVersion ) )
                        return false;

                    minVersion = parsedVersion;
                }
                else if( trimmed.StartsWith( "<" ) )
                {
                    if( maxVersion != null )
                        return false;

                    string versionStr = trimmed[1..];
                    if( !Version.TryParse( versionStr, out Version parsedVersion ) )
                        return false;

                    maxVersion = parsedVersion;
                }
                else
                {
                    return false;
                }
            }

            result = new VersionConstraint( minVersion, maxVersion );
            return true;
        }

        public static bool operator ==( VersionConstraint left, VersionConstraint right )
        {
            return left.MinVersion == right.MinVersion && left.MaxVersion == right.MaxVersion;
        }

        public static bool operator !=( VersionConstraint left, VersionConstraint right )
        {
            return left.MinVersion != right.MinVersion || left.MaxVersion != right.MaxVersion;
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