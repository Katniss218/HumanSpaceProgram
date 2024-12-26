using HSP.Input;
using HSP.ScreenCapturing;
using HSP.Settings;
using HSP.Time;
using HSP.Timelines;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    public static class OnStartup
    {
        public const string BIND_INPUT_CHANNELS = HSPEvent.NAMESPACE_HSP + ".bind_input_channels";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, BIND_INPUT_CHANNELS )]
        private static void BindInputs()
        {
            HierarchicalInputChannel.BindInputs();
        }

        public const string LOAD_SETTINGS_FROM_FILE = HSPEvent.NAMESPACE_HSP + ".settings.load";

        [HSPEventListener( HSPEvent_STARTUP_EARLY.ID, LOAD_SETTINGS_FROM_FILE )]
        private static void LoadSettingsFromFileOnStartup()
        {
            SettingsManager.ReloadSettings();
        }


        public const string ADD_TIME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_time_manager";
        public const string ADD_TIMELINE_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_timeline_manager";
        public const string ADD_SCREENSHOT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_screenshot_manager";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_TIME_MANAGER )]
        private static void AddTimeManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimeManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_TIMELINE_MANAGER )]
        private static void AddTimelineManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimelineManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_SCREENSHOT_MANAGER )]
        private static void AddScreenshotManager()
        {
            ScreenshotManager sm = AlwaysLoadedManager.GameObject.AddComponent<ScreenshotManager>();
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }

    }
}