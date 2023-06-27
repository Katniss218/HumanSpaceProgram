using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.Utils
{
    public static class IOUtils
    {
        const string MOD_DIRECTORY = "GameData";

        /// <summary>
        /// Figures out and returns the path to the `GameData` directory.
        /// </summary>
        public static string GetGameDataPath()
        {
            RuntimePlatform platform = Application.platform;
            string path = Application.dataPath;

            switch( platform )
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                    path = Directory.GetParent( path ).FullName; // + "/../";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                    path = Directory.GetParent( path ).FullName; // + "/../../";
                    break;
                case RuntimePlatform.OSXPlayer:
                    path = Directory.GetParent( path ).FullName; // + "/../";
                    break;
            }

            path = Path.Combine( path, MOD_DIRECTORY );
            return path;
        }
    }
}
