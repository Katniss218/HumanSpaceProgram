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
        /// Computes the path to the base directory where Human Space Program is installed.
        /// </summary>
        public static string GetBaseDirectoryPath()
        {
            string dataPath = Application.dataPath;

            switch( Application.platform )
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                    dataPath = Directory.GetParent( dataPath ).FullName; // "/../";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                    dataPath = Directory.GetParent( dataPath ).FullName; // "/../";
                    break;
                case RuntimePlatform.OSXPlayer:
                    dataPath = Directory.GetParent( dataPath ).Parent.FullName; // "/../../";
                    break;
            }

            return dataPath;
        }

        /// <summary>
        /// The name of the `Saves` directory.
        /// </summary>
        public const string SavesDirectoryName = "Saves";

        /// <summary>
        /// Figures out and returns the path to the `Saves` directory.
        /// </summary>
        public static string GetSaveDirectoryPath()
        {
            string path = Path.Combine( GetBaseDirectoryPath(), SavesDirectoryName );

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
            string path = Path.Combine( GetBaseDirectoryPath(), VesselsDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }
    }
}