using HSP.Core;
using HSP.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPlus.Input;

namespace HSP
{
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

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, Input_TakeScreenshot );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.COMMON_SCREENSHOT, Input_TakeScreenshot );
        }

        private bool Input_TakeScreenshot( float _ )
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