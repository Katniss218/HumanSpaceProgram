using System;
using System.IO;
using UnityEngine;

namespace HSP
{
    /// <summary>
    /// Takes and saves screenshots.
    /// </summary>
    public class ScreenshotManager : SingletonMonoBehaviour<ScreenshotManager>
    {
        public const string ScreenshotsDirectoryName = "Screenshots";

        /// <summary>
        /// Figures out and returns the path to the `Screenshots` directory.
        /// </summary>
        public static string GetScreenshotDirectoryPath()
        {
            string path = Path.Combine( ApplicationUtils.GetBaseDirectoryPath(), ScreenshotsDirectoryName );

            if( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );

            return path;
        }

        public bool TakeScreenshot()
        {
            string dirPath = GetScreenshotDirectoryPath();
            if( !Directory.Exists( dirPath ) )
            {
                Directory.CreateDirectory( dirPath );
            }

            DateTimeOffset date = DateTimeOffset.Now;

            string name = $"{date:yyyy-MM-dd_HH.mm.sszzz}.png";
            name = name.Replace( ":", "" ); // removes the `:` from the timezone.

            ScreenCapture.CaptureScreenshot( Path.Combine( dirPath, name ), 1 );
            return false;
        }
    }
}