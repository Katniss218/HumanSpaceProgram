using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Version = HSP.Content.Version;

namespace HSP.Content
{
    /// <summary>
    /// Provides utilities relating to the structure of HSP's content / files.
    /// </summary>
    public class HumanSpaceProgramContent
    {
        /// <summary>
        /// The name of the folder that contains the external game assets, including mods.
        /// </summary>
        public const string CONTENT_DIRECTORY_NAME = "GameData";

        /// <summary>
        /// Computes the path to the directory where the external game assets, including mods, are located.
        /// </summary>
        /// <remarks>
        /// This includes the 'vanilla' content.
        /// </remarks>
        public static string GetContentDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), CONTENT_DIRECTORY_NAME );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }

        /// <summary>
        /// Gets the list of all mod directories present in the current installation, keyed by the corresponding mod ID.
        /// </summary>
        /// <remarks>
        /// Some or all of the directories that are returned might be empty, or otherwise unused. Use with care.
        /// </remarks>
        public static IEnumerable<string> GetAllModDirectories()
        {
            return Directory.GetDirectories( GetContentDirectoryPath() );
        }

        /// <summary>
        /// Gets the mod ID that is associated with the given directory.
        /// </summary>
        /// <param name="modDirectory">The path to the root of the mod directory.</param>
        public static string GetModID( string modDirectory )
        {
            return Path.GetFileName( modDirectory );
        }

        public static string GetModDirectory( string modId )
        {
            return Path.Combine( GetContentDirectoryPath(), modId );
        }

        /// <summary>
        /// Gets the root directory of the mod that is associated with the given path.
        /// </summary>
        /// <param name="assetPath">The path to an asset inside the content directory.</param>
        public static string GetModDirectoryFromAssetPath( string assetPath )
        {
            string relPath = Path.GetRelativePath( GetContentDirectoryPath(), assetPath );
            string[] parts = relPath.Split( Path.DirectorySeparatorChar );
            if( parts.Length < 2 )
                return null;
            string modPath = Path.Combine( parts[1..] );
            return modPath;
        }

        /// <summary>
        /// Gets the root directory of the mod that is associated with the given path.
        /// </summary>
        /// <param name="assetPath">The path to an asset inside the content directory.</param>
        public static string GetModDirectoryFromAssetPath( string assetPath, out string modId )
        {
            string relPath = Path.GetRelativePath( GetContentDirectoryPath(), assetPath );
            string[] parts = relPath.Split( Path.DirectorySeparatorChar );
            if( parts.Length < 2 )
            {
                modId = parts[0];
                return null;
            }
            modId = parts[0];
            string modPath = Path.Combine( parts[1..] );
            return modPath;
        }

        /// <summary>
        /// Gets the asset ID that is associated with the given path.
        /// </summary>
        /// <param name="assetPath">The path to get the asset ID of. Must be inside the content directory.</param>
        public static string GetAssetID( string assetPath )
        {
            string relPath = GetModDirectoryFromAssetPath( assetPath, out string modId ).Replace( "\\", "/" ).Split( "." )[0];
            return $"{modId}::{relPath}";
        }

        /// <summary>
        /// Gets the asset path that's associated with the given asset ID. The extension is provided separately, as asset IDs don't use it.
        /// </summary>
        public static string GetAssetPath( string assetId, string extension )
        {
            string[] split = assetId.Split( "::" );
            string modId = split[0];
            return Path.Combine( GetModDirectory( modId ), split[1] ) + "." + extension;
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