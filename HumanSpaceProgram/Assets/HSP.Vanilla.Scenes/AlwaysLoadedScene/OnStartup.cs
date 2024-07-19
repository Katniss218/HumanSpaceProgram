using HSP.Input;
using HSP.ScreenCapturing;
using HSP.Time;
using HSP.Timelines;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    public static class OnStartup
    {
        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_HSP + ".add_time_manager" )]
        private static void AddTimeManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimeManager>();
        }
        
        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_HSP + ".add_timeline_manager" )]
        private static void AddTimelineManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimelineManager>();
        }

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_HSP + ".add_screenshot_manager" )]
        private static void AddScreenshotManager()
        {
            ScreenshotManager sm = AlwaysLoadedManager.GameObject.AddComponent<ScreenshotManager>();
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }
    }
}