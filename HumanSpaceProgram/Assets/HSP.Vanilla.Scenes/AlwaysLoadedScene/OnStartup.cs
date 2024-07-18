using HSP.Core;
using HSP.Input;
using HSP.ScreenCapturing;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    public static class OnStartup
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_screenshot_manager" )]
        private static void AddScreenshotManager()
        {
            ScreenshotManager sm = AlwaysLoadedManager.GameObject.AddComponent<ScreenshotManager>();
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }
    }
}