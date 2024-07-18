using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HSP
{
    public static class IOHelper
    {
        readonly static string[] FORBIDDEN_FILE_NAMES =
        {
            "con", "prn", "aux", "nul",
            "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
            "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
        };

        /// <summary>
        /// Sanitizes the user-provided name into a valid cross-platform filename.
        /// </summary>
        /// <remarks>
        /// Remember to check if a file with the returned name already exists before saving to it (!)
        /// </remarks>
        public static string SanitizeFileName( string rawFileName )
        {
            if( string.IsNullOrEmpty( rawFileName ) )
            {
                return "___";
            }

            const string FORBIDDEN_CHARS = "[^a-z0-9]";

            string sanitizedName = rawFileName.ToLowerInvariant().Trim();

            sanitizedName = Regex.Replace( sanitizedName, FORBIDDEN_CHARS, "_" );

            // Sanitize if the input name matches a forbidden name exactly.
            if( FORBIDDEN_FILE_NAMES.Contains( sanitizedName ) )
            {
                sanitizedName = $"_{sanitizedName}";
            }

            const int maxLength = 32; // conservative length limit because length limit is for absolute paths, ours is just a filename (`12345678901234567890123456789012`).
            if( sanitizedName.Length > maxLength )
            {
                sanitizedName = sanitizedName.Substring( 0, maxLength );
            }

            return sanitizedName;
        }
    }
}