using System;

namespace HSP.Timelines
{
    /// <summary>
    /// Specifies that a method should be used to migrate save data from one version to another.
    /// </summary>
    /// <remarks>
    /// This attribute can be applied to any static method with the signature `static void Method(SerializedData data)`.
    /// The method will be called to migrate save data from the specified fromVersion to the toVersion.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class SaveMigrationAttribute : Attribute
    {
        /// <summary>
        /// The ID of the mod this migration belongs to.
        /// </summary>
        public string ModID { get; }

        // Versions can't be actual `Version` structs due to C# attribute limitations unfortunately.

        /// <summary>
        /// The version to migrate from (as string, e.g., "1.0").
        /// </summary>
        public string FromVersion { get; }

        /// <summary>
        /// The version to migrate to (as string, e.g., "1.1").
        /// </summary>
        public string ToVersion { get; }

        /// <summary>
        /// Optional description of what this migration does.
        /// </summary>
        public string Description { get; set; }

        public SaveMigrationAttribute( string modId, string fromVersion, string toVersion )
        {
            if( string.IsNullOrEmpty( modId ) )
                throw new ArgumentException( "ModID cannot be null or empty", nameof( modId ) );
            if( string.IsNullOrEmpty( fromVersion ) )
                throw new ArgumentException( "FromVersion cannot be null or empty", nameof( fromVersion ) );
            if( string.IsNullOrEmpty( toVersion ) )
                throw new ArgumentException( "ToVersion cannot be null or empty", nameof( toVersion ) );

            if( fromVersion == toVersion )
                throw new ArgumentException( "FromVersion and ToVersion cannot be the same" );

            if( !Version.TryParse( fromVersion, out _ ) )
                throw new ArgumentException( "FromVersion is not a valid version string", nameof( fromVersion ) );
            if( !Version.TryParse( toVersion, out _ ) )
                throw new ArgumentException( "ToVersion is not a valid version string", nameof( toVersion ) );

            this.ModID = modId;
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
        }
    }
}
