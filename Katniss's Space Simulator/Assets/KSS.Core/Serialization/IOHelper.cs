using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assets.KSS.Core.Serialization
{
    public static class IOHelper
    {
        public static string GeneratePathID( string name )
        {
            if( string.IsNullOrEmpty( name ) )
            {
                return "___";
            }

            string sanitizedName = name.ToLowerInvariant();

            const string allowedChars = "[^a-zA-Z0-9]";
            sanitizedName = Regex.Replace( sanitizedName, allowedChars, "_" );

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

            const int maxLength = 32;
            if( sanitizedName.Length > maxLength )
            {
                sanitizedName = sanitizedName.Substring( 0, maxLength );
            }

            return sanitizedName;

            // We should append a number if file is already taken. But that's the responsibility of the exception handler that checks the directory.
        }
    }
}
