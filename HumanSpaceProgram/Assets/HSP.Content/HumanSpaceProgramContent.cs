using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HSP.Content
{
    /// <summary>
    /// Contains utility methods relating to the structure of HSP's content / files.
    /// </summary>
    public class HumanSpaceProgramContent
    {
        public const string ContentDirectoryName = "GameData";

        /// <summary>
        /// Computes the path to the directory where mods and mod content is located.
        /// </summary>
        /// <remarks>
        /// This includes the external 'vanilla' content as well.
        /// </remarks>
        public static string GetContentDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), ContentDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }

        /// <summary>
        /// Gets the list of all mod directories present in the current installation.
        /// </summary>
        /// <remarks>
        /// Some or all of the directories that are returned might be empty, or otherwise unused. Use with care.
        /// </remarks>
        public static IEnumerable<string> GetAllModDirectories()
        {
            return Directory.GetDirectories( GetContentDirectoryPath() );
        }


        public const string SavesDirectoryName = "Saves";

        /// <summary>
        /// Computes the path to the directory where game saves are located.
        /// </summary>
        public static string GetSaveDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), SavesDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }

        public const string VesselsDirectoryName = "Vessels";

        /// <summary>
        /// Computes the path to the directory where saved vessels are located.
        /// </summary>
        public static string GetSavedVesselsDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), VesselsDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }
    }
}