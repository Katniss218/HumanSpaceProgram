using System.IO;
using System.Reflection;
using UnityEngine;

namespace HSP.Content
{
    /// <summary>
    /// Constants regarding mods for HSP.
    /// </summary>
    public class HumanSpaceProgramContent
    {
        /// <summary>
        /// The name of the `GameData` directory.
        /// </summary>
        public const string GameDataDirectoryName = "GameData";

        /// <summary>
        /// Computes the path to the directory containing mods.
        /// </summary>
        public static string GetContentDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), GameDataDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }
    }
}