using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A static helper class for random stuff regarding the structure of the Human Space Program application.
    /// </summary>
    public static class HumanSpaceProgram
    {
        /// <summary>
        /// The name of the `Saves` directory.
        /// </summary>
        public const string SavesDirectoryName = "Saves";

        /// <summary>
        /// Figures out and returns the path to the `Saves` directory.
        /// </summary>
        public static string GetSaveDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), SavesDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }

        /// <summary>
        /// The name of the `Vessels` directory.
        /// </summary>
        public const string VesselsDirectoryName = "Vessels";

        /// <summary>
        /// Figures out and returns the path to the `Saves` directory.
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