using System.IO;
using UnityEngine;

namespace HSP.Settings
{
    /// <summary>
    /// Constants regarding HSP's settings.
    /// </summary>
    public class HumanSpaceProgramSettings
    {
        public const string SettingsFileName = "settings.json";

        /// <summary>
        /// Computes the path to the directory containing mods.
        /// </summary>
        public static string GetSettingsFilePath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), SettingsFileName );

            return path;
        }
    }
}