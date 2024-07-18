using HSP.Core;
using HSP.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    public static class OnStartup
    {
        public static void AddScreenshotManager()
        {
            ScreenshotManager sm = AlwaysLoadedManager.GameObject.AddComponent<ScreenshotManager>();
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }
    }
}