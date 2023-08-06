using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KSS.Core.Serialization
{
    public static class IOHelper
    {
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

            string sanitizedName = rawFileName.ToLowerInvariant();

            const string charsToReplace = "[^a-zA-Z0-9]";
            sanitizedName = Regex.Replace( sanitizedName, charsToReplace, "_" );

            string[] forbiddenNames =
            { "CON", "PRN", "AUX", "NUL",
              "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
              "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            // Sanitize if the input name matches a forbidden name exactly.
            if( forbiddenNames.Contains( sanitizedName ) )
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
