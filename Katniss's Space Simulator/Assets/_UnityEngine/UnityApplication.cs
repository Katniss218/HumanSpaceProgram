using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class ApplicationUtils
    {
        /// <summary>
        /// Computes the path to the base directory where the unity executable is located.
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
    }
}
